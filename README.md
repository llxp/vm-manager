# VM-Manager #
A simple azure functions app to manage the power state of an azure vm
This project belongs to the [Linux Server Configuration Project](https://github.com/llxp/Udacity_LinuxServerConfiguration_Project)<br/>
The frontend of the app can be found in [this repository](https://github.com/llxp/VM_Manager_Frontend)

## Setup of the Azure Functions ##
To create an **Azure Functions App** in Azure, go to the azure portal: [portal.azure.com](https://portal.azure.com)<br/>
If you haven't created a Subscription yet, create a [free subscription](https://azure.microsoft.com/en-us/free/).

If you are logged in to the portal, click on **Create a resource** and select the category **Compute**.<br/>
Then click on **Function App**<br/>
- **App name**: unique name for the functions app (will be your url to access the api from the frontend)
- Subscription: **Your Azure subscription**
- [Resource group](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-overview#resource-groups): Select existing from the list or create a new.
- OS: The operating system you want to use for the function app. If you want to use the .net Framework (not .net core) then you need to select Windows. Linux is currently in preview.
- Hosting Plan: [**Consumtion plan**](https://docs.microsoft.com/en-us/azure/azure-functions/functions-scale#how-the-consumption-plan-works)
- Location: [**North Europe**](https://azure.microsoft.com/en-us/global-infrastructure/regions/)
- Runtime Stack: **.Net**
- Storage: The [storage account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-overview) you want to use
- Application Insights: Useful, if you want to have detailed error reporting and statistics. If you want to use it, click on "Application Insights Disabled".
	- On the next screen click from "Disable" to "**Enable**"
	- Select **Create new resource**
	- New resource name: a unique name for the insights resource.
	- Location: **North Europe**
	- Click on **Apply**
- Click on **Create**
- You need to wait about 30 minutes before the deployment can begin. Otherwise, you can get problems with the deployment.

## Deployment ##
You can deploy this project directly from Visual Studio.<br/>
If you have cloned the repository, open the solutions file.
- In Visual Studio Click on **Build** -> **Publish VM_Manager**
- In the now opened screen, Click on "New profile..."
	- Select **Select existing**
	- Select On lower right edge, instead of "Publish Immediately" -> "**Create Profile**" and click on it.
	- Select your Subscription and the created functions app from the list (expand the resource group)
	- Click on **OK**
	- Select your created profile from the list
- Click on **Publish**
