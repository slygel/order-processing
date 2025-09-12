import { useQuery, useMutation } from "@apollo/client/react";
import { GET_ORDERS, GET_PRODUCTS } from "../graphql/queries";
import { CREATE_ORDER, CREATE_PRODUCT } from "../graphql/mutations";
import { useState } from "react";
import { Send, Plus } from "lucide-react";
import { toast } from "react-toastify";
import { Modal, Button } from "../components/common/Modal";
import { validateProduct, formatPrice } from "../utils/productValidation";
import type { 
    Product, 
    ProductsData, 
    CreateOrderCommandInput, 
    CreateOrderResponse, 
    CreateProductCommandInput,
} from "../types/product";

const ProductPage = () => {
    const { data, loading, error } = useQuery<ProductsData>(GET_PRODUCTS);
    const [selectedProducts, setSelectedProducts] = useState<Product[]>([]);
    const [showOrderModal, setShowOrderModal] = useState(false);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [orderError, setOrderError] = useState<string | null>(null);
    const [newProduct, setNewProduct] = useState<CreateProductCommandInput>({
        productName: '',
        price: 0
    });
    const [fieldErrors, setFieldErrors] = useState<{
        productName?: string;
        price?: string;
    }>({});

    const [createOrder, { loading: orderLoading }] = useMutation<CreateOrderResponse, { command: CreateOrderCommandInput }>(CREATE_ORDER, {
        refetchQueries: [{ query: GET_ORDERS }],
        onCompleted: () => {
            setShowOrderModal(false);
            setSelectedProducts([]);
            setOrderError(null);
            toast.success('Order created successfully!');
        },
        onError: (error) => {
            setOrderError(error.message);
        },
    });

    const [createProduct, { loading: createLoading }] = useMutation(CREATE_PRODUCT, {
        refetchQueries: [{ query: GET_PRODUCTS }],
        onCompleted: () => {
            setShowCreateModal(false);
            setNewProduct({ productName: '', price: 0 });
            toast.success('Product created successfully!');
        },
        onError: () => {
        },
    });

    if (loading) {
        return (
            <div className="text-center py-8">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-900 mx-auto"></div>
                <p className="mt-2 text-gray-600">Loading products...</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="text-center py-8 text-red-600">
                Failed to load products: {error.message}
            </div>
        );
    }

    const products = data?.products ?? [];

    const handleProductSelect = (product: Product) => {
        if (selectedProducts.find((p) => p.id === product.id)) {
            setSelectedProducts(selectedProducts.filter((p) => p.id !== product.id));
        } else {
            setSelectedProducts([...selectedProducts, { ...product, quantity: 1 }]);
        }
    };

    const handleQuantityChange = (productId: string, quantity: number) => {
        setSelectedProducts(
            selectedProducts.map((p) =>
                p.id === productId ? { ...p, quantity: Math.max(1, quantity) } : p
            )
        );
    };

    const handleOrderSubmit = async () => {
        if (selectedProducts.length === 0) return;

        try {
            const orderItems = selectedProducts.map(product => ({
                productId: product.id,
                quantity: product.quantity
            }));

            await createOrder({
                variables: {
                    command: {
                        items: orderItems
                    }
                }
            });
        } catch (err) {
            console.error(err);
        }
    };

    const handleCreateProduct = async () => {
        const validation = validateProduct(newProduct);
        setFieldErrors(validation.errors || {});
        
        if (!validation.isValid) {
            return;
        }

        const existing = products.find(
            (p) => p.productName.toLowerCase() === newProduct.productName.trim().toLowerCase()
        );

        if (existing) {
            setFieldErrors({ productName: "A product with this name already exists" });
            return;
        }

        try {
            await createProduct({
                variables: {
                    command: {
                        ...newProduct,
                        price: parseFloat(newProduct.price.toFixed(2))
                    }
                }
            });
            setFieldErrors({});
        } catch (err) {
            console.error(err);
            if (err instanceof Error) {
                setFieldErrors({ productName: err.message });
            } else {
                setFieldErrors({ productName: 'An unexpected error occurred' });
            }
        }
    };

    return (
        <div className="py-6 m-5">
            <div className="bg-white mb-6">
                <div className="mb-6 flex justify-between items-center">
                    <h2 className="text-3xl font-bold text-gray-800">
                        Product Catalog
                    </h2>
                    <button
                        onClick={() => setShowCreateModal(true)}
                        className="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700 flex items-center gap-2"
                    >
                        <Plus size={20} />
                        Create Product
                    </button>
                </div>

                {products.length === 0 ? (
                    <div className="text-center py-8">
                        <p className="text-gray-600">No products found.</p>
                    </div>
                ) : (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                        {products.map((product) => (
                            <div
                                key={product.id}
                                className="border rounded-lg overflow-hidden bg-white shadow-sm hover:shadow-md transition-shadow"
                            >
                                <div className="p-4">
                                    <h3 className="text-lg font-semibold text-gray-900 mb-2">
                                        {product.productName}
                                    </h3>
                                    <p className="text-gray-600 mb-2">
                                        Price: ${formatPrice(product.price)}
                                    </p>
                                    <p className="text-sm text-gray-400">
                                        Created at:{" "}
                                        {new Date(product.createdAt).toLocaleDateString()}
                                    </p>
                                    {selectedProducts.find((p) => p.id === product.id) ? (
                                        <div className="mt-2">
                                            <div className="flex items-center justify-between gap-2">
                                                <span className="text-sm text-gray-400">
                                                    Quantity:
                                                </span>
                                                <button
                                                    onClick={() => {
                                                        const selected = selectedProducts.find((p) => p.id === product.id);
                                                        if (selected) {
                                                            handleQuantityChange(product.id, selected.quantity - 1);
                                                        }
                                                    }}
                                                    className="px-3 py-1 rounded bg-gray-200 hover:bg-gray-300"
                                                >
                                                    -
                                                </button>
                                                <span className="text-gray-800">
                                                    {selectedProducts.find((p) => p.id === product.id)?.quantity || 1}
                                                </span>
                                                <button
                                                    onClick={() => {
                                                        const selected = selectedProducts.find((p) => p.id === product.id);
                                                        if (selected) {
                                                            handleQuantityChange(product.id, selected.quantity + 1);
                                                        }
                                                    }}
                                                    className="px-3 py-1 rounded bg-gray-200 hover:bg-gray-300"
                                                >
                                                    +
                                                </button>
                                                <button
                                                    onClick={() => handleProductSelect(product)}
                                                    className="px-3 py-1 rounded bg-red-100 text-red-800 hover:bg-red-200"
                                                >
                                                    Remove
                                                </button>
                                            </div>
                                        </div>
                                    ) : (
                                        <button
                                            onClick={() => handleProductSelect(product)}
                                            className="mt-3 w-full px-3 py-2 rounded-md text-sm font-medium transition bg-gray-100 text-gray-800 border-gray-500 hover:bg-gray-200"
                                        >
                                            Select
                                        </button>
                                    )}
                                </div>
                            </div>
                        ))}
                    </div>
                )}

                {/* Create Product Modal */}
                <Modal
                    isOpen={showCreateModal}
                    onClose={() => {
                        setShowCreateModal(false);
                        setNewProduct({ productName: '', price: 0 });
                    }}
                    title="Create New Product"
                >
                    <div className="space-y-4">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                Product Name
                            </label>
                            <input
                                type="text"
                                value={newProduct.productName}
                                onChange={(e) => {
                                    setNewProduct({ ...newProduct, productName: e.target.value });
                                    setFieldErrors((prev) => ({ ...prev, productName: undefined }));
                                }}
                                className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-1 focus:ring-blue-500 ${
                                    fieldErrors.productName ? 'border-red-500' : ''
                                }`}
                                placeholder="Enter product name"
                            />
                            {fieldErrors.productName && (
                                <p className="mt-1 text-sm text-red-600">
                                    {fieldErrors.productName}
                                </p>
                            )}
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                Price ($)
                            </label>
                            <input
                                type="number"
                                value={newProduct.price}
                                onChange={(e) => {
                                    const value = parseFloat(e.target.value);
                                    setNewProduct({ ...newProduct, price: isNaN(value) ? 0 : value });
                                    setFieldErrors((prev) => ({ ...prev, price: undefined }));
                                }}
                                className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-1 focus:ring-blue-500 ${
                                    fieldErrors.price ? 'border-red-500' : ''
                                }`}
                                placeholder="Enter price"
                                min="0"
                                step="1"
                            />
                            {fieldErrors.price && (
                                <p className="mt-1 text-sm text-red-600">
                                    {fieldErrors.price}
                                </p>
                            )}
                        </div>
                    </div>
                    <div className="mt-6 flex justify-end space-x-2">
                        <Button
                            variant="secondary"
                            onClick={() => {
                                setShowCreateModal(false);
                                setNewProduct({ productName: '', price: 0 });
                                setFieldErrors({});
                            }}
                        >
                            Cancel
                        </Button>
                        <Button
                            variant="primary"
                            onClick={handleCreateProduct}
                            isLoading={createLoading}
                        >
                            Create Product
                        </Button>
                    </div>
                </Modal>

                {/* Order Modal */}
                {showOrderModal && (
                    <div className="fixed inset-0 bg-[rgba(0,0,0,0.5)] bg-opacity-50 flex items-center justify-center p-4">
                        <div className="bg-white rounded-lg p-6 max-w-md w-full">
                            <h3 className="text-lg font-semibold mb-4">
                                Confirm Product Selection
                            </h3>
                            <div className="mb-4">
                                <p className="text-gray-600 mb-2 font-semibold">
                                    Selected Products:
                                </p>
                                <ul className="space-y-2">
                                    {selectedProducts.map((product) => (
                                        <li
                                            key={product.id}
                                            className="flex justify-between items-center"
                                        >
                                            <div>
                                                <span>{product.productName}</span>
                                                <span className="ml-2 text-gray-500">
                                                    (Qty: {product.quantity})
                                                </span>
                                            </div>
                                            <button
                                                onClick={() => handleProductSelect(product)}
                                                className="text-red-600 hover:text-red-800"
                                            >
                                                Remove
                                            </button>
                                        </li>
                                    ))}
                                </ul>
                            </div>
                            {orderError && (
                                <div className="mb-4 p-3 bg-red-100 text-red-700 rounded">
                                    {orderError}
                                </div>
                            )}
                            <div className="flex justify-end space-x-2">
                                <button
                                    onClick={() => {
                                        setShowOrderModal(false);
                                        setOrderError(null);
                                    }}
                                    className="cursor-pointer px-4 py-2 text-gray-600 hover:text-gray-800"
                                >
                                    Cancel
                                </button>
                                <button
                                    onClick={handleOrderSubmit}
                                    disabled={orderLoading || selectedProducts.length === 0}
                                    className="cursor-pointer px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50"
                                >
                                    {orderLoading ? "Submitting..." : "Submit Request"}
                                </button>
                            </div>
                        </div>
                    </div>
                )}

                {/* Submit Button */}
                {selectedProducts.length > 0 && (
                    <div className="fixed top-20 right-6">
                        <button
                            onClick={() => setShowOrderModal(true)}
                            className="bg-green-600 text-white px-6 py-3 rounded-full shadow-lg hover:bg-green-700 flex items-center gap-2"
                        >
                            <Send size={20} />
                            Submit Order ({selectedProducts.length})
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
};

export default ProductPage;
