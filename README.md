# O365 Photo Crawler

Universal Windows Platform (UWP) application that crawls and displays Office 365 user profiles and photos using Microsoft Graph API. Retrieves organizational user directory data including profile pictures, contact information, and organizational hierarchy.

Built for DX Readiness initiative to demonstrate Microsoft Graph API capabilities.

## Features

- Azure AD authentication via ADAL
- Microsoft Graph API integration
- User directory listing from Office 365
- Profile photo retrieval and display
- User detail pages with full profile information
- Adaptive UI for different window sizes
- Progress indicators for async operations
- Local settings caching

## Tech Stack

- **Platform**: Universal Windows Platform (UWP)
- **Language**: C#, XAML
- **APIs**: Microsoft Graph API, Azure Active Directory Authentication Library (ADAL)
- **Framework**: .NET Framework, Windows 10 SDK
- **Authentication**: OAuth 2.0, Azure AD

## Prerequisites

- Windows 10 (version 10240 or higher)
- Visual Studio 2015 or later with UWP development tools
- Office 365 tenant for testing
- Azure AD app registration

## Setup

### 1. Register Application in Azure AD

1. Go to Azure Portal → Azure Active Directory → App registrations
2. Create new registration:
   - Name: O365 Photo Crawler
   - Supported account types: Single tenant
   - Redirect URI: urn:ietf:wg:oauth:2.0:oob (Native client)
3. Note the Application (client) ID
4. Grant API permissions:
   - Microsoft Graph → Delegated permissions
   - User.Read (Read user profile)
   - User.ReadBasic.All (Read all users' basic profiles)
   - User.Read.All (Read all users' full profiles)

### 2. Configure Application

1. Open `App.xaml` in Visual Studio
2. Add resource dictionary entry:

```xml
<Application.Resources>
    <x:String x:Key="ida:ClientID">YOUR_CLIENT_ID_HERE</x:String>
    <x:String x:Key="ida:AADInstance">https://login.microsoftonline.com/</x:String>
    <x:String x:Key="ida:Tenant">common</x:String>
</Application.Resources>
```

### 3. Build and Run

1. Open `O365-Photo-Crawler.csproj` in Visual Studio
2. Restore NuGet packages (Microsoft.Graph, ADAL)
3. Set target to Local Machine or Device
4. Press F5 to build and run

## Usage

1. Launch application
2. Sign in with Office 365 credentials
3. Application retrieves user list from organization
4. Browse user directory with profile photos
5. Click user to view detailed profile information
6. Navigate back to user list

## API Permissions Required

- **User.Read**: Sign in and read user profile
- **User.ReadBasic.All**: Read all users' basic profiles (name, photo)
- **User.Read.All**: Read all users' full profiles (requires admin consent)

## Architecture

- **MainPage**: User directory list view with authentication
- **UserDisplayPage**: Split-view detail page for individual users
- **UserOperations**: Microsoft Graph API operations (GetUsersAsync, GetUserAsync)
- **AuthenticationHelper**: ADAL authentication flow management
- **GraphService**: Microsoft Graph client wrapper

## License

MIT License (Copyright Microsoft - see source file headers)

## Links

- Blog post: [O365 Photo Crawler: Microsoft Graph API Demo App](https://www.tanchunsiong.com/2016/01/o365-photo-crawler-microsoft-graph-api-demo-app/)
- GitHub: [github.com/tanchunsiong](https://github.com/tanchunsiong)
- LinkedIn: [linkedin.com/in/tanchunsiong](https://linkedin.com/in/tanchunsiong)
- X: [x.com/tanchunsiong](https://x.com/tanchunsiong)

**Project Created:** 2015-2016 (DX Readiness period)
