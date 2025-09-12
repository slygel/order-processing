import { gql } from "@apollo/client";

export const CREATE_ORDER = gql`
  mutation CreateOrder($command: CreateOrderCommandInput!) {
    createOrder(command: $command) {
      id
      createdAt
      status
      totalAmount
    }
  }
`;

export const CREATE_PRODUCT = gql`
  mutation CreateProduct($command: CreateProductCommandInput!) {
    createProduct(command: $command) {
      id
      productName
      price
      createdAt
    }
  }
`;