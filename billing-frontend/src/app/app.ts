import { Component, signal } from '@angular/core';
// 👇 Add RouterLink and RouterLinkActive to this import statement 👇
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CustomerFormComponent } from "./customer-form/customer-form";
import { ItemForm } from './item-form/item-form';

@Component({
  selector: 'app-root',
  // 👇 Register them here so app.html knows how to use routerLink 👇
  imports: [RouterOutlet, RouterLink, RouterLinkActive],  
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('stockz-frontend');
}