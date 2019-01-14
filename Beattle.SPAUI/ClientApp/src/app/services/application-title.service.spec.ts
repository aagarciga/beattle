import { TestBed, inject } from '@angular/core/testing';

import { ApplicationTitleService } from './application-title.service';

describe('ApplicationTitleService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ApplicationTitleService]
    });
  });

  it('should be created', inject([ApplicationTitleService], (service: ApplicationTitleService) => {
    expect(service).toBeTruthy();
  }));
});
