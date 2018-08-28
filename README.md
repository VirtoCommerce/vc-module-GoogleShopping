# GoogleShopping.MerchantModule
GoogleShopping.MerchantModule allows to sync Google Merchant Center products information with VirtoCommerce store products.

Key features:
* Retrieves Google Merchant Center product statuses;
* Uploads VirtoCommerce store products to Google Merchant Center.

# Documentation
User guide: 

Developer guide:

# Installation
Installing the module:
* Automatically: in VC Manager go to Configuration -> Modules -> Order module -> Install
* Manually: download module zip package from [Releases](https://github.com/VirtoCommerce/vc-module-GoogleShopping/releases) page. In VC Manager go to Configuration -> Modules -> Advanced -> upload module package -> Install.

# Settings
GoogleShopping.MerchantModule has the following settings:
* **GoogleShopping.Merchant.MerchantId** - ID of Google Merchant Center account that will be associated with this store;
* **GoogleShopping.MerchantModule.Description** - Description of Google Shopping for public site;
* **GoogleShopping.MerchantModule.LogoUrl** - URL of Google Shopping logo.

Also, to use it you will need to set up Google Merchant Center account information in Web.config:
* **GoogleShopping:MerchantAccount:Email** - email of service account that will be used for Google Merchant Center integration.
* **GoogleShopping:MerchantAccount:Certificate.Path** - full physical path to certificate key file linked with service account. Key file must be a valid P12 certificate (usually its filename ends with `.p12`).
* **GoogleShopping:MerchantAccount:Certificate.Passphrase** - passphrase for certificate key file mentioned above.

Example of filled data:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <!-- ... -->

    <appSettings>
        <!-- ... -->

        <add key="GoogleShopping:MerchantAccount:Email" value="xxxxx@developer.gserviceaccount.com" />
        <add key="GoogleShopping:MerchantAccount:Certificate.Path" value="C:\Some\secret\place\for\key.p12" />
        <add key="GoogleShopping:MerchantAccount:Certificate.Passphrase" value="notasecret" />
    </appSettings>

    <!-- ... -->
</configuration>
```

# License
Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.