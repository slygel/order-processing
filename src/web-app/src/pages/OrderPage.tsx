import AppLayout from "../components/AppLayout"
import OrderList from "../components/OrderList";

const OrderPage = () => {
    return (
        <AppLayout>
            <div className="p-6 min-h-screen">
                <OrderList />
            </div>
        </AppLayout>
    )
};
export default OrderPage;