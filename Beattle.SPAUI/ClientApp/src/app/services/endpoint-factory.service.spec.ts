import { TestBed, inject } from '@angular/core/testing';

import { EndpointFactoryService } from './endpoint-factory.service';

describe('EndpointFactoryService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [EndpointFactoryService]
    });
  });

  it('should be created', inject([EndpointFactoryService], (service: EndpointFactoryService) => {
    expect(service).toBeTruthy();
  }));
});
