import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { CustomerFormComponent } from '../customer-form/customer-form';

@Component({
  selector: 'app-bill-entry',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HttpClientModule],
  templateUrl: './bill-entry.html',
  styleUrls: ['./bill-entry.css']
})
export class BillEntryComponent implements OnInit {
  billForm!: FormGroup;
  customers: any[] = [];
  items: any[] = []; // Your item inventory from database

  successMessage: string = '';
  errorMessage: string = '';

  private baseUrl = 'https://localhost:44366/api';

  constructor(private fb: FormBuilder, private http: HttpClient) {}

  ngOnInit() {
    this.initForm();
    this.loadDropdownData();
  }

  private initForm() {
    this.billForm = this.fb.group({
      customerId: ['', Validators.required],
      phoneNo: [{ value: '', disabled: true }],
      address: [{ value: '', disabled: true }],
      
      // 👇 Dynamic collection array container for multiple row items
      itemsArray: this.fb.array([]), 

      subTotal: [{ value: 0, disabled: true }],
      gstPercent: [0, [Validators.required, Validators.min(0)]],
      grandTotal: [{ value: 0, disabled: true }]
    });

    // Automatically add the very first item row on page load
    this.addItemRow();

    // Listen to changes on the overall form to recalculate the whole bill
    this.billForm.valueChanges.subscribe(() => {
      this.calculateBillTotals();
    });
  }

  // Helper getter to easily access the rows array in HTML
  get itemsFormArray(): FormArray {
    return this.billForm.get('itemsArray') as FormArray;
  }

  // Instantiates a brand new row entry group
  addItemRow() {
    const row = this.fb.group({
      itemId: ['', Validators.required],
      unitPrice: [{ value: 0, disabled: true }],
      quantity: [1, [Validators.required, Validators.min(1)]],
      totalAmount: [{ value: 0, disabled: true }]
    });

    // Listen to individual row changes to update that specific row's totalAmount
    row.valueChanges.subscribe(() => {
      this.calculateRowAmount(row);
    });

    this.itemsFormArray.push(row);
  }

  // Removes a specific row item if the user misclicks
  removeItemRow(index: number) {
    if (this.itemsFormArray.length > 1) {
      this.itemsFormArray.removeAt(index);
    }
  }

  private loadDropdownData() {
    this.http.get<any[]>(`https://localhost:44366/api/Customer/get-customers`).subscribe(data => this.customers = data);
    this.http.get<any[]>(`https://localhost:44366/api/Item/get-items`).subscribe(data => this.items = data);
  }

  onCustomerSelect(event: Event) {
    // 👇 1. Keep this as a standard string instead of trying to convert it with "+"
    const selectedId = (event.target as HTMLSelectElement).value; 
    
    // 👇 2. Match the string identifiers directly
    const matchedCustomer = this.customers.find(c => c.customerId === selectedId);

    if (matchedCustomer) {
      this.billForm.patchValue({
        phoneNo: matchedCustomer.phoneNo,
        address: matchedCustomer.address || 'No address specified' // Fallback for your empty address rows
      });
    }
  }

  // 🔄 Fire patch whenever a user picks an item inside a specific index row
  onItemSelect(event: Event, index: number) {
    const selectedId = +(event.target as HTMLSelectElement).value;
    const matchedItem = this.items.find(i => i.itemId === selectedId);
    const rowGroup = this.itemsFormArray.at(index) as FormGroup;

    if (matchedItem) {
      rowGroup.patchValue({ unitPrice: matchedItem.unitPrice }, { emitEvent: false });
      this.calculateRowAmount(rowGroup);
    }
  }

  // Calculates [Price * Qty] for a single line item row
  private calculateRowAmount(row: FormGroup) {
    const price = row.get('unitPrice')?.value || 0;
    const qty = row.get('quantity')?.value || 0;
    const total = price * qty;
    
    row.patchValue({ totalAmount: parseFloat(total.toFixed(2)) }, { emitEvent: false });
  }

  // Aggregates all active item rows together and appends GST
  private calculateBillTotals() {
    let subTotal = 0;

    // Sum up totals from all dynamic rows
    this.itemsFormArray.controls.forEach((control) => {
      subTotal += control.get('totalAmount')?.value || 0;
    });

    const gstPercent = this.billForm.get('gstPercent')?.value || 0;
    const gstAmount = subTotal * (gstPercent / 100);
    const grandTotal = subTotal + gstAmount;

    this.billForm.patchValue({
      subTotal: parseFloat(subTotal.toFixed(2)),
      grandTotal: parseFloat(grandTotal.toFixed(2))
    }, { emitEvent: false });
  }

  onSubmitBill() {
    if (this.billForm.invalid) return;

    const payload = this.billForm.getRawValue();
    console.log('Submitting Bill:', payload);
        
    // 👇 Notice we added <any> so TypeScript lets us read the custom backend properties safely
    this.http.post<any>(`${this.baseUrl}/Billing/save-bill`, payload).subscribe({
      next: (response) => {
        this.successMessage = 'Invoice finalized! Starting PDF download...';
        
        // ⚡ AUTOMATIC TRIGGER: Call the download method using the real ID from the database!
        if (response && response.billId) {
          this.executePdfDownload(response.billId);
        } else {
          console.error('Backend saved the bill but did not return a valid billId.');
        }

        // Clean out form inputs safely for the next bill entry
        this.itemsFormArray.clear(); 
        this.billForm.reset({ quantity: 1, gstPercent: 0 }); 
        this.addItemRow(); 

        setTimeout(() => this.successMessage = '', 4000);
      },
      error: () => {
        this.errorMessage = 'Failed to submit billing records.';
        setTimeout(() => this.errorMessage = '', 4000);
      }
    });
  }

  // ⚡ Internal helper that handles the raw binary blob download stream
  private executePdfDownload(billId: string) {
    this.http.get(`${this.baseUrl}/Billing/download-pdf/${billId}`, { responseType: 'blob' })
      .subscribe({
        next: (blobData: Blob) => {
          // Wrap file stream data into a browser-executable link download hook
          const fileUrl = window.URL.createObjectURL(blobData);
          const downloadAnchor = document.createElement('a');
          downloadAnchor.href = fileUrl;
          downloadAnchor.download = `Invoice_${billId.substring(0, 8)}.pdf`;
          
          document.body.appendChild(downloadAnchor);
          downloadAnchor.click();
          
          // Cleanup memory tracking allocation frames safely
          document.body.removeChild(downloadAnchor);
          window.URL.revokeObjectURL(fileUrl);
        },
        error: () => {
          this.errorMessage = "Invoice saved, but could not download the PDF generation stream.";
          setTimeout(() => this.errorMessage = '', 4000);
        }
      });
  }

  // 📄 Action: Tied to your manual "Download PDF" button if a user wants to re-download
  onDownloadPDF() {
    // For manual button click, you can grab the current selected customer context or active parameters,
    // but the auto-download on save is now fully covered above!
    console.log("Manual download clicked");
  }

  // 📄 Action: Compiles active form inputs and exports an invoice PDF file
  // onDownloadPDF() {
  //   // You'll need the generated bill ID from your database to fetch the PDF. 
  //   // Usually, you fetch this when clicking 'Finalize' or tracking it locally.
  //   const billId = "019e31c7-a7b3-7c17-8703-6891c1128aee"; // Replace with actual active record reference index
    
  //   this.http.get(`${this.baseUrl}/Bill/download-pdf/${billId}`, { responseType: 'blob' })
  //     .subscribe({
  //       next: (blobData: Blob) => {
  //         // Wrap file stream data into a browser-executable click link download hook
  //         const fileUrl = window.URL.createObjectURL(blobData);
  //         const downloadAnchor = document.createElement('a');
  //         downloadAnchor.href = fileUrl;
  //         downloadAnchor.download = `Invoice_${billId.substring(0, 8)}.pdf`;
          
  //         document.body.appendChild(downloadAnchor);
  //         downloadAnchor.click();
          
  //         // Cleanup memory tracking allocation frames safely
  //         document.body.removeChild(downloadAnchor);
  //         window.URL.revokeObjectURL(fileUrl);
  //       },
  //       error: () => this.errorMessage = "Could not fetch compiled invoice document stream."
  //     });
  // } 

  //Action: Resets the entire form and array state cleanly
  onClearForm() {
    // 1. Fully empty out the dynamic dynamic item lines array tracking block
    this.itemsFormArray.clear();
    
    // 2. Clear out the rest of the controls back to pristine default architecture state
    this.billForm.reset({
      customerId: '',
      phoneNo: '',
      address: '',
      gstPercent: 0,
      subTotal: 0,
      grandTotal: 0
    });

    // 3. Re-append exactly one blank row item entry layout line so the UI isn't empty
    this.addItemRow();
    
    this.successMessage = "Billing board fields cleared successfully.";
    setTimeout(() => this.successMessage = '', 3000);
  }
}