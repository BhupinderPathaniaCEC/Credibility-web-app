import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RateWebsiteComponent } from './rate-website.component';


describe('RateWebsiteComponent', () => {
  let component: RateWebsiteComponent;
  let fixture: ComponentFixture<RateWebsiteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RateWebsiteComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RateWebsiteComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
