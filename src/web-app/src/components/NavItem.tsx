import {Link} from "react-router-dom";
import type { NavItemProps } from "../types/NavItemProps";

const NavItem = ({ icon, title, href, isActive }: NavItemProps) => {
    return (
        <Link
            to={href}
            className={`flex items-center gap-2 px-3 py-2 text-sm font-medium transition-colors hover:text-gray-900 dark:hover:text-gray-100"
                ${isActive ? "text-gray-900 dark:text-gray-100" : "text-gray-600 dark:text-gray-400"}`}
        >
            {icon}
            <span className={"text-base"}>{title}</span>
        </Link>
    )
}

export default NavItem