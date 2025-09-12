import { useLazyQuery, useQuery } from "@apollo/client/react";
import { Package2, Loader2, X } from 'lucide-react';
import { GET_ORDER_BY_ID, GET_ORDERS } from '../graphql/queries';
import { FormatDate } from "../helpers/FormatDate";
import { useState } from "react";

type OrderItem = {
    productId: string;
    productName: string;
    quantity: number;
    unitPrice: number;
    totalPrice: number;
};

type Order = {
    id: string;
    createdAt: string;
    totalAmount: number;
    status: string;
};

type OrderDetails = {
    orderById: {
        id: string;
        createdAt: string;
        status: string;
        totalAmount: number;
        items: OrderItem[];
    };
};

export default function OrderList() {

    const { loading: loadingOrders, error: ordersError, data } = useQuery<{ orders: Order[] }>(GET_ORDERS);

    const [selectedOrderId, setSelectedOrderId] = useState<string | null>(null);
    const [getOrderById, { data: orderDetails, loading: loadingOrderDetails }] = useLazyQuery<OrderDetails>(GET_ORDER_BY_ID);

    const handleViewOrder = (orderId: string) => {
        setSelectedOrderId(orderId);
        getOrderById({ variables: { id: orderId } });
        console.log("Fetching details for order ID:", orderId);
    };

    const closeModal = () => {
        setSelectedOrderId(null);
    };

    if (loadingOrders) {
        return (
            <div className="flex justify-center items-center min-h-[400px]">
                <Loader2 className="w-8 h-8 animate-spin text-blue-500" />
            </div>
        );
    }

    if (ordersError) {
        return (
            <div className="p-6">
                <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                    <span className="text-red-600">Error: {ordersError.message}</span>
                </div>
            </div>
        );
    }

    return (
        <div className="p-6">
            <div className="flex justify-between items-center mb-6">
                <div className="flex items-center gap-2">
                    <Package2 className="w-6 h-6" />
                    <h1 className="text-2xl font-semibold">My Orders</h1>
                </div>
            </div>

            <div className="overflow-x-auto rounded-lg border border-gray-200">
                <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                        <tr>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Order ID</th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Created At</th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Total Amount</th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                        </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                        {data?.orders.map((order: Order) => (
                            <tr key={order.id} className="hover:bg-gray-50">
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <button
                                        onClick={() => handleViewOrder(order.id)}
                                        className="text-blue-600 hover:text-blue-800 underline cursor-pointer"
                                    >
                                        {order.id.split('-')[0]}
                                    </button>
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap">
                                    {FormatDate(order.createdAt)}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap">
                                    ${order.totalAmount.toFixed(2)}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full
                                    ${order.status.toLowerCase() === 'paid'
                                            ? 'bg-green-100 text-green-800'
                                            : order.status.toLowerCase() === 'pending'
                                                ? 'bg-yellow-100 text-yellow-800'
                                                : 'bg-red-100 text-red-800'
                                        }`}
                                    >
                                        {order.status}
                                    </span>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {/* Order Details Modal */}
            {selectedOrderId && (
                <div className="fixed inset-0 bg-[rgba(0,0,0,0.5)] bg-opacity-40 flex justify-center items-center p-4 z-50">
                    <div className="bg-white rounded-lg shadow-lg max-w-lg w-full p-6 relative">
                        {/* Close Button */}
                        <button
                            onClick={closeModal}
                            className="absolute top-3 right-3 text-gray-500 hover:text-gray-700"
                        >
                            <X className="w-5 h-5" />
                        </button>

                        <h2 className="text-xl font-semibold mb-4 underline">Order Details</h2>

                        {loadingOrderDetails ? (
                            <div className="flex justify-center items-center py-10">
                                <Loader2 className="w-6 h-6 animate-spin text-blue-500" />
                            </div>
                        ) : orderDetails?.orderById ? (
                            <div>
                                <p className="mb-2">
                                    <span className="font-medium">Order ID:</span>{" "}
                                    <span className="ml-1">{orderDetails.orderById.id.split('-')[0]}</span>
                                </p>
                                <p className="mb-2">
                                    <span className="font-medium">Created At:</span>{" "}
                                    <span className="ml-1">{FormatDate(orderDetails.orderById.createdAt)}</span>
                                </p>
                                <p className="mb-2">
                                    <span className="font-medium">Status:</span>{" "}
                                    <span
                                        className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full
                                        ${orderDetails.orderById.status.toLowerCase() === "paid"
                                                ? "bg-green-100 text-green-800"
                                                : orderDetails.orderById.status.toLowerCase() === "pending"
                                                    ? "bg-yellow-100 text-yellow-800"
                                                    : "bg-red-100 text-red-800"}`}
                                    >
                                        {orderDetails.orderById.status}
                                    </span>
                                </p>

                                <p className="mb-2">
                                    <span className="font-medium">Total Amount:</span>
                                    <span className="ml-2">${orderDetails.orderById.totalAmount.toFixed(2)}</span>
                                </p>

                                <h3 className="text-lg font-semibold mb-2">Items</h3>
                                <table className="min-w-full border">
                                    <thead className="bg-gray-100">
                                        <tr>
                                            <th className="px-3 py-2 text-left text-sm font-medium text-gray-600">
                                                Product
                                            </th>
                                            <th className="px-3 py-2 text-left text-sm font-medium text-gray-600">
                                                Quantity
                                            </th>
                                            <th className="px-3 py-2 text-left text-sm font-medium text-gray-600">
                                                Unit Price
                                            </th>
                                            <th className="px-3 py-2 text-left text-sm font-medium text-gray-600">
                                                Total
                                            </th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {orderDetails.orderById.items.map((item: any) => (
                                            <tr key={item.productId} className="border-t">
                                                <td className="px-3 py-2">{item.productName}</td>
                                                <td className="px-3 py-2">{item.quantity}</td>
                                                <td className="px-3 py-2">${item.unitPrice.toFixed(2)}</td>
                                                <td className="px-3 py-2">${item.totalPrice.toFixed(2)}</td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        ) : (
                            <p className="text-gray-500">No details found.</p>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
}
