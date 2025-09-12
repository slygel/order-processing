import React from "react";

interface NavItemProps {
    icon: React.ReactNode
    title: string
    href: string
    isActive?: boolean
}

export type { NavItemProps };