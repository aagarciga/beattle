import { TestBed, inject } from '@angular/core/testing';

import { AccountEndPointService } from './account-endpoint.service';

describe('AccountEndPointService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AccountEndPointService]
    });
  });

  it('should be created', inject([AccountEndPointService], (service: AccountEndPointService) => {
    expect(service).toBeTruthy();
  }));
});
