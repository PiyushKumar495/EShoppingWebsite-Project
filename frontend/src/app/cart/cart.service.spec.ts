import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CartService, CartResponse } from './cart.service';

// Jasmine unit test for CartService
describe('CartService', () => {
  let service: CartService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [CartService]
    });
    service = TestBed.inject(CartService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should fetch cart from backend if logged in', () => {
    spyOn(localStorage, 'getItem').and.returnValue('token');
    const mockCart: CartResponse = { cartId: 1, items: [], granDtotal: 0 };
    service.getCart().subscribe(cart => {
      expect(cart).toEqual(mockCart);
    });
    const req = httpMock.expectOne('/api/cart');
    expect(req.request.method).toBe('GET');
    req.flush(mockCart);
  });

  it('should fetch cart from localStorage if not logged in', (done) => {
    spyOn(localStorage, 'getItem').and.returnValue(null);
    const guestCart: CartResponse = { cartId: 0, items: [], granDtotal: 0 };
    // Use Object.defineProperty to mock getLocalCart
    Object.defineProperty(service as any, 'getLocalCart', { value: () => guestCart });
    service.getCart().subscribe(cart => {
      expect(cart).toEqual(guestCart);
      done();
    });
  });
});
