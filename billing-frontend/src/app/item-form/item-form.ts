import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-item-form',
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './item-form.html',
  styleUrl: './item-form.css',
})
export class ItemForm {
  itemForm: FormGroup;
  successMessage: string = '';
  errorMessage: string = '';

  private apiUrl = 'https://localhost:44366/api/Item'; 

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.itemForm = this.fb.group({
      itemName: ['', [Validators.required, Validators.minLength(2)]],

      unitPrice: ['', [Validators.required, Validators.min(0.01), Validators.pattern('^[0-9]*\\.?[0-9]{0,2}$')]]
    });
  }

  onSubmit() {
    if (this.itemForm.invalid) return;

    this.successMessage = '';
    this.errorMessage = '';

    this.http.post<any>(this.apiUrl + '/save-item', this.itemForm.value).subscribe({
      next: (res) => {
        this.successMessage = `Item "${this.itemForm.value.itemName}" added successfully!`;
        this.itemForm.reset();
        setTimeout(() => this.clearAlerts(), 4000);
      },
      error: () => {
        this.errorMessage = 'Could not save item records to the backend.';
        setTimeout(() => this.clearAlerts(), 4000);
      }
    });
  }

  clearAlerts() {
    this.successMessage = '';
    this.errorMessage = '';
  }
}
