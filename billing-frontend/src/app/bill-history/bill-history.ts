import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common'; 
import { FormsModule } from '@angular/forms'; 

@Component({
  selector: 'app-bill-history',
  templateUrl: './bill-history.html',
  styleUrls: ['./bill-history.css'],
  standalone: true,
  imports: [CommonModule, FormsModule] 
})
export class BillHistoryComponent implements OnInit {
  private baseUrl = 'https://localhost:44366/api'; // Check your exact backend port!
  
  invoiceList: any[] = [];
  
  // These form fields bind directly to your HTML [(ngModel)] parameters
  filters = {
    customerName: '',
    phoneNo: '',
    fromDate: '',
    toDate: ''
  };

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadHistory(); 
  }

  loadHistory() {
    // 👇 PREPARATION: Convert empty strings to null so C# Nullable properties work correctly
    const requestPayload = {
      customerName: this.filters.customerName.trim() || null,
      phoneNo: this.filters.phoneNo.trim() || null,
      fromDate: this.filters.fromDate || null,
      toDate: this.filters.toDate || null
    };

    // Make the backend call sending the optimized payload structure
    this.http.post<any[]>(`${this.baseUrl}/Billing/history`, requestPayload).subscribe({
      next: (data) => {
        this.invoiceList = data;
        console.log('History loaded successfully:', data);
      },
      error: (err) => {
        console.error('Failed to pull billing tracking registry list', err);
      }
    });
  }

  resetFilters() {
    this.filters = { customerName: '', phoneNo: '', fromDate: '', toDate: '' };
    this.loadHistory();
  }

  downloadPdf(billId: string) {
    this.http.get(`${this.baseUrl}/Billing/download-pdf/${billId}`, { responseType: 'blob' })
      .subscribe((blobData: Blob) => {
        const fileUrl = window.URL.createObjectURL(blobData);
        const downloadAnchor = document.createElement('a');
        downloadAnchor.href = fileUrl;
        downloadAnchor.download = `Invoice_${billId.substring(0, 8).toUpperCase()}.pdf`;
        document.body.appendChild(downloadAnchor);
        downloadAnchor.click();
        document.body.removeChild(downloadAnchor);
        window.URL.revokeObjectURL(fileUrl);
      });
  }
}