import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { ChartComponent } from './chart.component';

let component: ChartComponent;
let fixture: ComponentFixture<ChartComponent>;

describe('chart component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ChartComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(ChartComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});
