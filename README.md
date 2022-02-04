# Overview

Middleware repository to authorise and direct traffic to the correct checkout provider.

# Providers

| Name | Auth Type | Status |
|:- |:- |:- |
| Amazon SP API | OAUTH | Pending Verification |
| BigCommerce | OAUTH | Pending Tests |
| Demandware | OAUTH | Worst platform ever |
| eBay | OAUTH | Pending Tests |
| Ecwid | OAUTH | Pending Dev Store Test |
| Etsy API v3 | OAUTH | Pending Tests |
| Magento 2 API | OAUTH | Awaiting Email |
| Shopify | OAUTH | Complete |
| Squarespace | OAUTH | Complete |
| WooCommerce API | OAUTH | Pending Tests |
| Walmart | OAUTH |
| Shift4Shop (3DCart) | OAUTH | Pending Dev Store Test |
| AspDotNetStoreFront | Basic |
| CommerceHQ eCommerce | Basic | Awaiting Email |
| Hybris | Basic | Registered |
| LightSpeed eCommerce | Basic |
| Neto | Basic |
| Shopware API | Basic |
| Volusion | Basic | Complete |
| AceShop | Bridge File | Pending UI |
| Cscart | Bridge File | Pending UI |
| Cubecart | Bridge File | Pending UI |
| Gambio | Bridge File | Pending UI |
| Loaded Commerce | Bridge File | Pending UI |
| Interspire | Bridge File | Pending UI |
| JooCart | Bridge File | Pending UI |
| Magento | Bridge File | Pending UI |
| MijoShop | Bridge File | Pending UI |
| OpenCart | Bridge File | Pending UI |
| OscMax | Bridge File | Pending UI |
| Oscommerce | Bridge File | Pending UI |
| Oxid | Bridge File | Pending UI |
| Pinnacle | Bridge File | Pending UI |
| PrestaShop | Bridge File | Pending UI |
| Shop-Script Premium | Bridge File | Pending UI |
| Shopware | Bridge File | Pending UI |
| Tomatocart | Bridge File | Pending UI |
| Ubercart | Bridge File | Pending UI |
| Virtuemart | Bridge File | Pending UI |
| WPecommerce | Bridge File | Pending UI |
| WooCommerce | Bridge File | Pending UI |
| WebAsyst | Bridge File | Pending UI |
| Xcart | Bridge File | Pending UI |
| Xtcommerce | Bridge File | Pending UI |
| XtcommerceVeyton | Bridge File | Pending UI |
| Zen Cart | Bridge File | Pending UI |

# Authentication

All requests to generate a callback URL require the following parameters

- shop

- userId

- entityId

- airslipUserType

## Shopify

`shop` should be in the format of {shop-name}.myshopify.com

Requires an additional parameter

- isOnline

## Squarespace

### Dev store

Name: `airslip-development`

Request OAUTH access through their application form at https://partner.squarespace.com/oauth-form. Applications can take up to 7 days and Squarespace will review the registration and respond with client_id and client_secret as soon as possible.

TODO:
Complete steps from docs - https://developers.squarespace.com/oauth

## BigCommerce

### Dev store

Must install from BigCommerce website.

Name: `store-5fflrx2ogq`

Link: https://store-5fflrx2ogq.mybigcommerce.com/manage/marketplace/apps/36851

> NOTE: State parameter is not passed in the callback.

## Ecwid

[Development Management Website](https://my.ecwid.com/store/71467012#develop-apps)

> NOTE: State parameter is not passed in the callback.

## Woo Commerce

Provider Name: `WoocommerceApi`

Store name format is **www.{example}.com**. The API Keys and Secrets are hosted on the customers website. The callback url needs to be over SSL.

## Shift4Shop (3DCart)

[Development Management Website](https://devportal.3dcart.com/app.asp?ut1q=27a&c7=SxZm2%2F5JRPs%3D)


## Lightspeed eCommerce

### How does authentication work?

Authentication is managed via HTTP authentication. Every request must include an HTTP Authorization Header.

Below is the URL template for every API call: 

`https://{api_key}:{api_secret}@{cluster_url}/{shop_language}/{resource}.json` 

### Instructions

- https://developers.lightspeedhq.com/ecom/tutorials/build-an-app/ 
- https://developers.lightspeedhq.com/ecom/introduction/authentication/


## Magento 2 API

Create integration and authentication instructions

https://devdocs.magento.com/guides/v2.4/get-started/create-integration.html 