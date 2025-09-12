import { Bell, Home, LogOut,Search, Settings, ShoppingBasket, Sprout, User } from "lucide-react"
import { Link } from "react-router-dom"
import { useState } from "react"
import NavItem from "../NavItem"

const Header = () => {

    const [isMenuOpen, setIsMenuOpen] = useState(false)

    const toggleMenu = () => {
        setIsMenuOpen(!isMenuOpen)
    }

    const NavItems = () => (
        <>
            <NavItem icon={<Home className="h-6 w-6" />} title="Home" href="/" />
            <NavItem icon={<ShoppingBasket className="h-6 w-6" />} title="My Orders" href="/orders" />
        </>
    )

    return (
        <header className="sticky top-0 z-40 border-b-1 border-b-gray-100 shadow-sm bg-white">
            <div className="container mx-auto flex h-16 items-center justify-between px-4">
                {/* Logo */}
                <div className="flex items-center">
                    <Link to="/" className="flex items-center gap-2">
                        <Sprout className="h-8 w-8 text-emerald-600" />
                        <span className="text-xl font-bold">
                            OrderApp
                        </span>
                    </Link>
                </div>

                {/* Desktop Navigation */}
                <nav className="hidden md:flex items-center space-x-1">
                    {NavItems()}
                </nav>

                {/* Right side actions */}
                <div className="flex items-center gap-2">
                    <button className="cursor-pointer rounded-full p-2 text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800">
                        <Search className="h-5 w-5" />
                    </button>
                    <button className="cursor-pointer rounded-full p-2 text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800">
                        <Bell className="h-5 w-5" />
                    </button>

                    {/* User menu */}
                    <div className="relative">
                        <button
                            onClick={toggleMenu}
                            className="cursor-pointer flex h-8 w-8 items-center justify-center rounded-full bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600"
                        >
                            <User className="h-4 w-4 text-gray-600 dark:text-gray-400" />
                        </button>

                        {/* User dropdown menu */}
                        {isMenuOpen && (
                            <div className="absolute right-0 mt-2 w-48 rounded-md bg-white py-1 shadow-lg ring-1 ring-black ring-opacity-5 dark:bg-gray-800">
                                <div className="px-4 py-2 border-b dark:border-gray-700">
                                    <p className="text-sm font-medium">Tai Tue</p>
                                    <p className="text-xs text-gray-500 dark:text-gray-400">
                                        Member
                                    </p>
                                </div>
                                <Link
                                    to="/"
                                    className="flex items-center px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:text-gray-200 dark:hover:bg-gray-700"
                                >
                                    <User className="mr-2 h-4 w-4" />
                                    Profile
                                </Link>
                                <Link
                                    to="/"
                                    className="flex items-center px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:text-gray-200 dark:hover:bg-gray-700"
                                >
                                    <Settings className="mr-2 h-4 w-4" />
                                    Settings
                                </Link>
                                <button
                                    className="flex w-full items-center px-4 py-2 text-sm text-red-600 hover:bg-gray-100 dark:text-red-400 dark:hover:bg-gray-700"
                                >
                                    <LogOut className="mr-2 h-4 w-4" />
                                    Logout
                                </button>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </header>
    )
}

export default Header