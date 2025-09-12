import type { LayoutProps } from "../types/LayoutProps";
import Footer from "./layout/Footer";
import Header from "./layout/Header";

export default function AppLayout({ children }: LayoutProps) {

    return (
        <div className="flex min-h-screen flex-col bg-white dark:bg-gray-950">
            <Header/>

            {/* Main content */}
            <main>{children}</main>

            <Footer/>
        </div>
    )
}