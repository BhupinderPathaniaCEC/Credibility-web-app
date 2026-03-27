import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RateWebsite } from './rate-website';

describe('RateWebsite', () => {
  let component: RateWebsite;
  let fixture: ComponentFixture<RateWebsite>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RateWebsite]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RateWebsite);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
