import { TestBed, inject } from '@angular/core/testing';

import { NotificationEndpointService } from './notification-endpoint.service';

describe('NotificationEndpointService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [NotificationEndpointService]
    });
  });

  it('should be created', inject([NotificationEndpointService], (service: NotificationEndpointService) => {
    expect(service).toBeTruthy();
  }));
});
