import type { CreateProductCommandInput } from '../types/product';

interface ValidationResult {
    isValid: boolean;
    errors?: {
        productName?: string;
        price?: string;
    };
}

export const validateProduct = (product: CreateProductCommandInput): ValidationResult => {
    const errors: ValidationResult['errors'] = {};
    
    if (!product.productName.trim()) {
        errors.productName = 'Product name is required';
    }

    if (product.price <= 0) {
        errors.price = 'Price must be greater than 0';
    }

    return {
        isValid: Object.keys(errors).length === 0,
        errors
    };
};

export const formatPrice = (price: number): string => {
    return price.toFixed(2);
};
