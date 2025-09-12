import { gql } from "@apollo/client";

export const GET_ORDERS = gql`
  query Orders {
    orders {
        id
        createdAt
        status
        totalAmount
    }
  }
`;

export const GET_ORDER_BY_ID = gql`
  query OrderById($id: UUID!) {
    orderById(id: $id) {
      id
      createdAt
      status
      totalAmount
      items {
          productId
          productName
          quantity
          unitPrice
          totalPrice
      }
    }
  }
`;

export const GET_PRODUCTS = gql`
  query {
    products {
        id
        productName
        price
        createdAt
    }
  }
`;
