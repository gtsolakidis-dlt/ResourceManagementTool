# Setting up Seq for Logging on Azure VM

This guide explains how to set up **Seq** on your Azure VM sandbox environment to collect and view structured logs from the Resource Management Tool.

## 1. Install Seq on the VM

1.  **Connect to your VM** via Remote Desktop (RDP).
2.  **Download Seq**:
    *   Open the browser on the VM.
    *   Go to: [https://datalust.co/download](https://datalust.co/download)
    *   Download the **Windows MSI** installer.
3.  **Run the Installer**:
    *   Follow the prompts. 
    *   When asked, choose **"Production"** or **"Development"** (Development is fine for a sandbox).
    *   The default port is **5341**. Keep this.
    *   Set an Administrator password when prompted (remember this!).
4.  **Verify**:
    *   On the VM, open Edge/Chrome and go to `http://localhost:5341`.
    *   You should see the Seq login screen.

## 2. Configure Networking (Allow Access)

To view the logs from your local developer machine (outside the VM), you need to open port **5341**.

### A. Azure Network Security Group (NSG)
1.  Go to the **Azure Portal** > Your VM > **Networking**.
2.  Click **Add inbound port rule**.
    *   **Source**: `Any` (or `My IP` for better security).
    *   **Source port ranges**: `*`
    *   **Destination**: `Any`
    *   **Service**: `Custom`
    *   **Destination port ranges**: `5341`
    *   **Protocol**: `TCP`
    *   **Action**: `Allow`
    *   **Priority**: `1010` (or any available low number).
    *   **Name**: `Allow_Seq`
3.  Click **Add**.

### B. Windows Firewall (Inside the VM)
1.  Inside the RDP session, open **PowerShell** as Administrator.
2.  Run the following command to allow traffic on port 5341:
    ```powershell
    New-NetFirewallRule -DisplayName "Allow Seq" -Direction Inbound -LocalPort 5341 -Protocol TCP -Action Allow
    ```

## 3. Verify Connection
1.  On your **local machine** (your laptop), open a browser.
2.  Navigate to `http://<YOUR_VM_PUBLIC_IP>:5341`.
3.  You should see the Seq login page. Login with the details you set during installation.

## 4. Application Configuration

The application is configured to log to `http://localhost:5341`. 

*   Since the API and Seq are running on the **same VM**, this default configuration will work automatically. The API talks to Seq locally.
*   You do **not** need to change the API configuration if they are on the same machine.

## 5. Troubleshooting

*   **Logs not appearing?**
    *   Check `logs/api-....log` on the VM to see if there are internal Serilog errors.
    *   Ensure the API has been restarted *after* Seq was installed.
*   **Cannot access UI?**
    *   Double-check the Azure NSG rule is applied to the correct Network Interface.
