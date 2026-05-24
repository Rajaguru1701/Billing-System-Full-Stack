import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HttpClientModule],
  templateUrl: './customer-form.html',
  styleUrls: ['./customer-form.css']
})
export class CustomerFormComponent {
  customerForm: FormGroup;
  successMessage: string = '';
  errorMessage: string = '';

  // Base API URL from your configuration profile port
  private apiUrl = 'https://localhost:44366/api/Customer';

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.customerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      phoneNo: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      address: ['']
    });
  }

 onSubmit() {
    if (this.customerForm.invalid) return;

    // Resets current alerts on click submission cycle
    this.successMessage = '';
    this.errorMessage = '';

    this.http.post<any>(this.apiUrl + '/save-customer', this.customerForm.value).subscribe({
      next: (res) => {
        // Captures your API return payload block
        this.successMessage = `Customer entry created successfully! Assigned ID: ${res.customerId}`;
        this.customerForm.reset();

        // Auto dismiss toast after 4 seconds
        setTimeout(() => this.clearAlerts(), 4000); 
      },
      error: () => {
        this.errorMessage = 'Could not sync records with database backend.';
        
        // Auto dismiss error toast after 4 seconds
        setTimeout(() => this.clearAlerts(), 4000);
      }
    });
  } // <-- This is where onSubmit safely ends!

  // Now clearAlerts sits cleanly as an independent method inside the class structure
  clearAlerts() {
    this.successMessage = '';
    this.errorMessage = '';
  }
}