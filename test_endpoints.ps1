# Endpoint tests for CommUnityHub
$ErrorActionPreference = "Stop"
$baseUrl = "http://localhost:5047"
$cookieFile = "test_cookies.txt"
if (Test-Path $cookieFile) { Remove-Item $cookieFile -Force }

Write-Host "Running CommUnityHub tests..."

function Assert-Endpoint {
    param(
        [string]$Name,
        [string]$Url,
        [string]$ExpectedPattern
    )
    $res = curl.exe -s -L "$baseUrl$Url"
    if ($res -match $ExpectedPattern) {
        Write-Host "  [PASS] $($Name) - $($Url)" -ForegroundColor Green
        return $true
    } else {
        Write-Host "  [FAIL] $($Name) - $($Url) (Pattern '$ExpectedPattern' not matched)" -ForegroundColor Red
        return $false
    }
}

# 1. Base Navigation Checks
Write-Host "`nStep 1: Verifying Core Navigation & Existing Features..."
Assert-Endpoint -Name "Home Page" -Url "/" -ExpectedPattern "CommUnity Hub"
Assert-Endpoint -Name "Categories Page" -Url "/Categories" -ExpectedPattern "Home Maintenance"
Assert-Endpoint -Name "Browse Services" -Url "/ServiceListings" -ExpectedPattern "Lawn Mowing"
Assert-Endpoint -Name "Service Details" -Url "/ServiceListings/Details/3" -ExpectedPattern "Lawn Mowing"

# 2. Third Entity: Orders & Shipment Management Checks
Write-Host "`nStep 2: Verifying Orders & Shipment Management..."
Assert-Endpoint -Name "Orders All" -Url "/Order" -ExpectedPattern "Order & Shipment Management"
Assert-Endpoint -Name "Orders Shipped Filter" -Url "/Order?status=shipped" -ExpectedPattern "NZ Post Local Courier"
Assert-Endpoint -Name "Orders Processing Filter" -Url "/Order?status=processing" -ExpectedPattern "CourierPost"
Assert-Endpoint -Name "Order Details #1" -Url "/Order/Details?orderId=1" -ExpectedPattern "NZP-88234190-WN"

# 3. Secure Shopping Cart Workflow
Write-Host "`nStep 3: Verifying Secure Shopping Cart (Session & Cookie Isolated)..."
# Add to Cart
$addRes = curl.exe -s -c $cookieFile -b $cookieFile -L "$baseUrl/Cart/AddToCart?listingId=3&count=1"
if ($addRes -match "Shopping Cart" -and $addRes -match "Lawn Mowing") {
    Write-Host "  [PASS] Add to Cart & Redirect to Index" -ForegroundColor Green
} else {
    Write-Host "  [FAIL] Add to Cart did not return expected Shopping Cart view" -ForegroundColor Red
}

# Inspect Cart Content & Badge
$cartView = curl.exe -s -c $cookieFile -b $cookieFile "$baseUrl/Cart"
if ($cartView -match 'badge bg-danger rounded-pill">1</span>') {
    Write-Host "  [PASS] Cart Badge renders count = 1" -ForegroundColor Green
} else {
    Write-Host "  [FAIL] Cart Badge count not matched" -ForegroundColor Red
}

# Test Cart Plus Quantity Increment
$cartIdMatch = [regex]::Match($cartView, 'href="/Cart/Plus\?cartId=([0-9]+)"')
if ($cartIdMatch.Success) {
    $cartId = $cartIdMatch.Groups[1].Value
    $plusRes = curl.exe -s -c $cookieFile -b $cookieFile -L "$baseUrl/Cart/Plus?cartId=$cartId"
    $cartViewAfterPlus = curl.exe -s -c $cookieFile -b $cookieFile "$baseUrl/Cart"
    if ($cartViewAfterPlus -match 'badge bg-danger rounded-pill">2</span>') {
        Write-Host "  [PASS] Quantity Increment (Plus) updated cart item #$cartId to badge = 2" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] Quantity increment failed" -ForegroundColor Red
    }
} else {
    Write-Host "  [FAIL] Could not find cartId in Cart view" -ForegroundColor Red
}

# 4. Checkout and Order Placement
Write-Host "`nStep 4: Verifying Summary Checkout & Order Placement..."
$summaryHtml = curl.exe -s -c $cookieFile -b $cookieFile "$baseUrl/Cart/Summary"
$tokenRegex = [regex]'name="__RequestVerificationToken" type="hidden" value="([^"]+)"'
$match = $tokenRegex.Match($summaryHtml)
if (-not $match.Success) {
    Write-Host "  [FAIL] CSRF Token not found on Summary page" -ForegroundColor Red
} else {
    $token = $match.Groups[1].Value
    Write-Host "  [PASS] Extracted CSRF Antiforgery Token" -ForegroundColor Green

    $checkoutOutput = curl.exe -s -i -c $cookieFile -b $cookieFile -L `
      --data-urlencode "__RequestVerificationToken=$token" `
      --data-urlencode "OrderHeader.Name=Tyler Resident" `
      --data-urlencode "OrderHeader.PhoneNumber=021-888-9999" `
      --data-urlencode "OrderHeader.StreetAddress=88 Campus Road" `
      --data-urlencode "OrderHeader.City=Lower Hutt" `
      --data-urlencode "OrderHeader.Suburb=Petone" `
      --data-urlencode "OrderHeader.PostalCode=5012" `
      "$baseUrl/Cart/Summary"

    if ($checkoutOutput -match "Order Confirmation" -or $checkoutOutput -match "Thank you for your order") {
        Write-Host "  [PASS] Checkout successfully submitted and redirected to OrderConfirmation" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] Checkout failed to redirect to OrderConfirmation" -ForegroundColor Red
    }
}

# 5. RESTful Payment API Verification
Write-Host "`nStep 5: Verifying RESTful Payment API Endpoints..."
# GET Order Status
$apiGetRes = curl.exe -s "$baseUrl/api/payment/order-status/1"
if ($apiGetRes -match '"orderStatus":"Shipped"' -and $apiGetRes -match '"trackingNumber":"NZP-88234190-WN"') {
    Write-Host "  [PASS] GET /api/payment/order-status/1 returned valid tracking payload" -ForegroundColor Green
} else {
    Write-Host "  [FAIL] GET /api/payment/order-status/1 returned invalid payload: $apiGetRes" -ForegroundColor Red
}

# POST Create Checkout Session
$postObj = @{
    orderHeaderId = 1
    returnUrlBase = $baseUrl
} | ConvertTo-Json

$apiPostRes = Invoke-RestMethod -Uri "$baseUrl/api/payment/create-checkout-session" -Method Post -ContentType "application/json" -Body $postObj
if ($apiPostRes.sessionId -and $apiPostRes.checkoutUrl) {
    Write-Host "  [PASS] POST /api/payment/create-checkout-session returned session & checkout URL (Session: $($apiPostRes.sessionId))" -ForegroundColor Green
} else {
    Write-Host "  [FAIL] POST /api/payment/create-checkout-session failed" -ForegroundColor Red
}

Write-Host "`nStep 6: Database State Inspection..."
sqlcmd -S "(localdb)\mssqllocaldb" -d "CommUnityHubDb" -Q "SELECT TOP 5 Id, Name, OrderTotal, OrderStatus, PaymentStatus, TrackingNumber FROM OrderHeaders ORDER BY Id DESC"

Write-Host "`nAll tests completed."
