# Overview

Middleware repository to authorise and direct traffic to the correct checkout provider.

# Providers

| Name | Auth Type | Status |
|:- |:- |:- |
| Shift4Shop REST API | OAUTH |
| Amazon | OAUTH |
| Amazon SP API | OAUTH |
| BigCommerce | OAUTH | Pending Tests |
| Demandware | OAUTH |
| eBay | OAUTH | Pending Tests |
| Ecwid | OAUTH |
| Etsy | OAUTH | Not Doing |
| Etsy API v3 | OAUTH | Pending Tests |
| Magento 2 API | OAUTH | Awaiting Email |
| Shopify | OAUTH | Complete |
| Squarespace | OAUTH | Awaiting Email |
| WooCommerce API | OAUTH | Pending Tests |
| Walmart | OAUTH |
| Shift4Shop | Basic |
| AspDotNetStoreFront | Basic |
| CommerceHQ eCommerce | Basic |
| Hybris | Basic |
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


Request OAUTH access through their application form at https://partner.squarespace.com/oauth-form. Applications can take up to 7 days and Squarespace will review the registration and respond with client_id and client_secret as soon as possible.

TODO:
Complete steps from docs - https://developers.squarespace.com/oauth

