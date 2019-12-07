/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { BmpSelectionAndOverviewComponent } from './bmp-selection-and-overview.component';

let component: BmpSelectionAndOverviewComponent;
let fixture: ComponentFixture<BmpSelectionAndOverviewComponent>;

describe('bmpSelectionAndOverview component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ BmpSelectionAndOverviewComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(BmpSelectionAndOverviewComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});