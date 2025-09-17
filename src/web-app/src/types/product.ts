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
    createProduct: {
        isSuccess: boolean;
        error?: string;
        statusCode: number;
        value: Product | null;
    };
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
