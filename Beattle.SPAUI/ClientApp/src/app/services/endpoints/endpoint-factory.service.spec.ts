import { TestBed, inject } from '@angular/core/testing';

import { EndPointFactoryService } from './endpoint-factory.service';

describe('EndPointFactoryService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [EndPointFactoryService]
    });
  });

  it('should be created', inject([EndPointFactoryService], (service: EndPointFactoryService) => {
    expect(service).toBeTruthy();
  }));
});
