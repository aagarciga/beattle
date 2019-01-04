import { TestBed, inject } from '@angular/core/testing';

import { LocalStoreManagerService } from './local-store-manager.service';

describe('LocalStoreManagerService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [LocalStoreManagerService]
    });
  });

  it('should be created', inject([LocalStoreManagerService], (service: LocalStoreManagerService) => {
    expect(service).toBeTruthy();
  }));
});
