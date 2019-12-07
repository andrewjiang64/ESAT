/// <reference path="../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { TreeviewComponent } from './treeview.component';

let component: TreeviewComponent;
let fixture: ComponentFixture<TreeviewComponent>;

describe('treeview component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ TreeviewComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(TreeviewComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});