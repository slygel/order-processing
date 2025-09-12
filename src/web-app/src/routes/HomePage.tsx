import AppLayout from "../components/AppLayout";
import ProductList from "../pages/ProductPage";

const HomePage = () => {
    return (
        <AppLayout>
            <div className="min-h-screen w-screen flex flex-col">
                <main className="flex-grow">
                    <section className="bg-gradient-to-r from-emerald-500 to-teal-600 text-white py-16">
                        <div className="container mx-auto px-4">
                            <div className="flex flex-col md:flex-row items-center justify-between">
                                <div className="md:w-1/2 mb-8 md:mb-0 ">
                                    <h1 className="text-4xl md:text-5xl font-bold mb-4">Discover Your Next Favorite Product</h1>
                                    <p className="text-lg mb-6">
                                        Access thousands of products, manage your orders, and discover new items all
                                        in one place.
                                    </p>
                                </div>
                                <div className="md:w-1/2 flex justify-center transition-opacity duration-500 h-72 w-72"></div>
                            </div>
                        </div>
                    </section>
                    <ProductList />
                </main>
            </div>
        </AppLayout>
    )
}

export default HomePage;