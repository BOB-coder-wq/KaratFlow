# Google OAuth 2.0 Setup Guide for Karat Flow

This guide is for developers setting up Google OAuth 2.0 for the Karat Flow Avalonia application.

**Important:** End users do NOT need to set up OAuth. Like Facebook, you (the developer) set up OAuth once, and all end users can use "Sign in with Google" without any setup.

## Step 1: Create a Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click on the project dropdown at the top
3. Click "New Project"
4. Enter a project name (e.g., "Karat Flow")
5. Click "Create"

## Step 2: Enable Google+ API

1. In the Google Cloud Console, go to "APIs & Services" > "Library"
2. Search for "Google+ API" or "Google Identity Platform"
3. Click on it and click "Enable"

## Step 3: Configure OAuth Consent Screen

1. Go to "APIs & Services" > "OAuth consent screen"
2. Choose "External" (for production) or "Internal" (for testing within your organization)
3. Click "Create"
4. Fill in the required fields:
   - **App name**: Karat Flow
   - **User support email**: your email
   - **Developer contact email**: your email
5. Click "Save and Continue"
6. Skip the Scopes section (click "Save and Continue")
7. Add test users if needed (for External setup)
8. Click "Save and Continue" then "Back to Dashboard"

## Step 4: Create OAuth 2.0 Credentials

1. Go to "APIs & Services" > "Credentials"
2. Click "Create Credentials" > "OAuth client ID"
3. Choose "Desktop application" as the application type
4. Name it "Karat Flow Desktop"
5. **Important**: Add the following redirect URI:
   - `http://localhost:8080/callback`
   - Or for production: `https://your-domain.com/callback`
6. Click "Create"

## Step 5: Copy Your Credentials

After creating the OAuth client, you'll see a popup with:
- **Client ID**: A long string like `123456789-abcdef.apps.googleusercontent.com`
- **Client Secret**: A shorter string like `GOCSPX-abcdef123456`

Copy both of these values.

## Step 6: Add Credentials to appsettings.Local.json

**Important:** Never commit secrets to git. Use `appsettings.Local.json` (gitignored) for development.

Open or create `appsettings.Local.json` and add the Google OAuth credentials:

```json
{
  "GoogleOAuth": {
    "ClientId": "your-client-id-here",
    "ClientSecret": "your-client-secret-here",
    "RedirectUri": "http://localhost:8080/callback"
  }
}
```

Replace:
- `your-client-id-here` with the Client ID from Step 5
- `your-client-secret-here` with the Client Secret from Step 5
- `http://localhost:8080/callback` with your redirect URI (must match what you set in Step 4)

## Step 7: Test the Setup

1. Rebuild and run the application
2. Click the "Sign in with Google" button
3. You should be redirected to Google's sign-in page
4. Sign in with your Google account
5. Grant permissions if prompted
6. You should be redirected back to the app and logged in

## For End Users

End users do NOT need to set up OAuth. They simply:
1. Download and run the app
2. Click "Sign in with Google"
3. Sign in with their Google account
4. Done!

This is how apps like Facebook work - the developer sets up OAuth once, and all users can use it without any setup.

## Troubleshooting

### Error: "redirect_uri_mismatch"
- Make sure the redirect URI in appsettings.json exactly matches what you set in the Google Cloud Console
- Check for trailing slashes or http vs https differences

### Error: "invalid_client"
- Verify your Client ID and Client Secret are correct
- Make sure you copied them correctly (no extra spaces)

### Error: "access_denied"
- Make sure you've added your email as a test user in the OAuth consent screen (for External setup)
- Or switch to Internal setup for testing within your organization

### Error: "unauthorized_client"
- Make sure you selected "Desktop application" as the application type when creating credentials
- For web apps, you might need to use "Web application" instead

## Security Notes

- **For development:** Credentials are in appsettings.Local.json (gitignored, never committed)
- **For production:** Use environment variables or embed credentials in the compiled app
- Keep your Client Secret secure
- Regularly rotate your credentials
- Never commit secrets to git (GitHub will block the push)

## Production Deployment

For production deployment, end users should NOT need to set up OAuth. You have two options:

### Option 1: Environment Variables (Recommended)
1. Set environment variables for Google OAuth credentials
2. The app reads from environment variables at runtime
3. End users just run the app - no setup needed

### Option 2: Embed Credentials in Compiled App
1. Embed your OAuth credentials in the compiled application
2. End users download the compiled app with credentials embedded
3. No configuration files needed on end user machines

For production deployment:
1. Change the redirect URI to your production domain
2. Complete the OAuth consent screen verification process
3. Add your production domain to the authorized domains
4. Use environment variables or embed credentials in the compiled app
5. Never commit production credentials to git

## Additional Resources

- [Google OAuth 2.0 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [Google Cloud Console](https://console.cloud.google.com/)
- [OAuth 2.0 Playground](https://developers.google.com/oauthplayground/)
