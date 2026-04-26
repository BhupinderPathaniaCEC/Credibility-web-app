import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WebsiteDetailsComponent } from './website-details.component';

describe('WebsiteDetailsComponent', () => {
  let component: WebsiteDetailsComponent;
  let fixture: ComponentFixture<WebsiteDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WebsiteDetails]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WebsiteDetails);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
