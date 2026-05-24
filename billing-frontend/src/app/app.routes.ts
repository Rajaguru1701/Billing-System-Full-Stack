import { Routes } from '@angular/router';
import { CustomerFormComponent } from './customer-form/customer-form';
import { ItemForm } from './item-form/item-form';
import { BillEntryComponent } from './bill-entry/bill-entry';
import { BillHistoryComponent } from './bill-history/bill-history';

export const routes: Routes = [
  { path: '', redirectTo: 'new-customer', pathMatch: 'full' },

  { path: 'new-customer', component: CustomerFormComponent },
  { path: 'bill-entry', component: BillEntryComponent },
  { path: 'bill-history', component: BillHistoryComponent },
  { path: 'add-item', component: ItemForm },
  { path: '**', redirectTo: 'new-customer' },
];
