import type { PropsWithChildren } from 'react';

interface ModalProps extends PropsWithChildren {
    isOpen: boolean;
    onClose: () => void;
    title: string;
}

export const Modal = ({ isOpen, title, children }: ModalProps) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-[rgba(0,0,0,0.5)] bg-opacity-50 flex items-center justify-center p-4">
            <div className="bg-white rounded-lg p-6 max-w-md w-full">
                <h3 className="text-lg font-semibold mb-4">{title}</h3>
                {children}
            </div>
        </div>
    );
};

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: 'primary' | 'secondary' | 'danger';
    isLoading?: boolean;
}

export const Button = ({ 
    variant = 'primary', 
    isLoading, 
    children, 
    className = '', 
    disabled,
    ...props 
}: ButtonProps) => {
    const baseStyles = 'px-4 py-2 rounded-md transition-colors';
    const variantStyles = {
        primary: 'bg-green-600 text-white hover:bg-green-700 disabled:opacity-50',
        secondary: 'text-gray-600 hover:text-gray-800',
        danger: 'bg-red-100 text-red-800 hover:bg-red-200'
    };

    return (
        <button
            className={`${baseStyles} ${variantStyles[variant]} ${className}`}
            disabled={isLoading || disabled}
            {...props}
        >
            {isLoading ? 'Loading...' : children}
        </button>
    );
};
