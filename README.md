# Overview

Middleware repository to authorise and direct traffic to the correct checkout provider.

# Providers

| Name | Auth Type |
|:- |:- |
| Shift4Shop REST API | OAUTH |
| Amazon | OAUTH |
| Amazon SP API | OAUTH |
| BigCommerce | OAUTH |
| Demandware | OAUTH |
| eBay | OAUTH |
| Ecwid | OAUTH |
| Etsy | OAUTH |
| Etsy API v3 | OAUTH |
| Magento 2 API | OAUTH |
| Shopify | OAUTH |
| Squarespace | OAUTH |
| WooCommerce API | OAUTH |
| Walmart | OAUTH |
| Shift4Shop | Basic |
| AspDotNetStoreFront | Basic |
| CommerceHQ eCommerce | Basic |
| Hybris | Basic |
| LightSpeed eCommerce | Basic |
| Neto | Basic |
| Shopware API | Basic |
| Volusion | Basic |
| AceShop | Bridge File |
| Cscart | Bridge File |
| Cubecart | Bridge File |
| Gambio | Bridge File |
| Loaded Commerce | Bridge File |
| Interspire | Bridge File |
| JooCart | Bridge File |
| Magento | Bridge File |
| MijoShop | Bridge File |
| OpenCart | Bridge File |
| OscMax | Bridge File |
| Oscommerce | Bridge File |
| Oxid | Bridge File |
| Pinnacle | Bridge File |
| PrestaShop | Bridge File |
| Shop-Script Premium | Bridge File |
| Shopware | Bridge File |
| Tomatocart | Bridge File |
| Ubercart | Bridge File |
| Virtuemart | Bridge File |
| WPecommerce | Bridge File |
| WooCommerce | Bridge File |
| WebAsyst | Bridge File |
| Xcart | Bridge File |
| Xtcommerce | Bridge File |
| XtcommerceVeyton | Bridge File |
| Zen Cart | Bridge File |

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

