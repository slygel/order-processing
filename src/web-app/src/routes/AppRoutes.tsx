import { BrowserRouter, Route, Routes } from "react-router-dom";
import HomePage from "./HomePage";
import OrderPage from "../pages/OrderPage";

const AppRoutes = () => {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<HomePage />} />
                <Route path="/orders" element={<OrderPage />} />
            </Routes>
        </BrowserRouter>
    )
}
export default AppRoutes;