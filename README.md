# Overview

Middleware repository to authorise and direct traffic to the correct checkout provider.

## Status's

- Registered
- Awaiting Response
- Pending Verification
- In Development
- Publish App Required
- Pending UI
- Pending App Approval
- Complete

# Providers

| Name | Auth Type | Status |
|:- |:- |:- |
| Amazon SP API | OAUTH | Pending App Approval |
| BigCommerce | OAUTH | Pending UI |
| Demandware | OAUTH | Awaiting Response |
| eBay | OAUTH | Publish App Required |
| Ecwid | OAUTH | Publish App Required |
| Etsy API v3 | OAUTH | Pending Verification |
| Magento 2 API | OAUTH | Awaiting Response |
| Shopify | OAUTH | Pending App Approval |
| Squarespace | OAUTH | Pending UI |
| WooCommerce API | OAUTH | Pending UI |
| Walmart | OAUTH | Awaiting Response |
| Shift4Shop (3DCart) | OAUTH | In Development |
| AspDotNetStoreFront | Basic |
| CommerceHQ eCommerce | Basic | 
| Hybris | Basic | Registered |
| LightSpeed eCommerce | Basic | Awaiting Email |
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

# Authentication -  Generation of callback URL

The following stores expect a shop parameter

- Shopify `airslip-development.myshopify.com`
- 3DCart `airslip-development`

The following stores expect a domain address passed in the shop parameter

- WooCommerceApi `https://www.airslip.com`

## Shopify

Has Marketplace: Yes

Marketplace URL: https://apps.shopify.com

Process: Create and manage account at https://partners.shopify.com

### Parameters

`shop` should be in the format of {shop-name}.myshopify.com

`timestamp` is in seconds sinch epoch

`hmac` is a generated value by Shopify

`isOnline` changes the grant_options[] value. per-user is for Shopify ecommerce provider and value is for Shopify POS.

### Dev store

Name: `airslip-development`

## Squarespace

Has Marketplace: Yes

Marketplace URL: https://www.squarespace.com/extensions/home

Auth Reference : https://developers.squarespace.com/oauth

Process: Request OAUTH access through their application form at https://partner.squarespace.com/oauth-form. Applications can take up to 7 days and Squarespace will review the registration and respond with client_id and client_secret as soon as possible. A new support request should then be sent to list the app on the Squarespace marketplace.

### Parameters

`shop` should be in the format of {shop-name}.

### Dev store

Name: `airslip-development`

## BigCommerce

### Dev store

Must install from BigCommerce website.

Name: `store-5fflrx2ogq`

Link: https://store-5fflrx2ogq.mybigcommerce.com/manage/marketplace/apps/36851

> NOTE: State parameter is not passed in the callback.

## Ecwid

[Development Management Website](https://my.ecwid.com/store/71467012#develop-apps)

> NOTE: State parameter is not passed in the callback.

An email to ec.apps@lightspeedhq.com is required to update settings.

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

## Amazon SP API

A user is required to be created in AWS with a policy and role.


