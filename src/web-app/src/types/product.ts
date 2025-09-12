export interface Product {
    id: string;
    productName: string;
    price: number;
    createdAt: string;
    quantity: number;
}

export interface ProductsData {
    products: Product[];
}

export interface CreateProductCommandInput {
    productName: string;
    price: number;
}

export interface CreateProductResponse {
    createProduct: Product;
}

export interface CreateOrderCommandInput {
    items: {
        productId: string;
        quantity: number;
    }[];
}

export interface CreateOrderResponse {
    createOrder: {
        id: string;
        createdAt: string;
        status: string;
        totalAmount: number;
    };
}
