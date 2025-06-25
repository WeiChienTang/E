#!/bin/bash
# Linux deployment script for ERP Core 2

echo "Starting ERP Core 2 deployment..."

# Stop existing service
sudo systemctl stop erpcore2 2>/dev/null || true

# Backup existing version
if [ -d "/var/www/erpcore2" ]; then
    sudo cp -r /var/www/erpcore2 "/var/www/erpcore2_backup_$(date +%Y%m%d_%H%M%S)"
fi

# Deploy new version
sudo rm -rf /var/www/erpcore2
sudo mkdir -p /var/www/erpcore2
sudo cp -r ./publish/linux-x64/* /var/www/erpcore2/

# Set permissions
sudo chown -R www-data:www-data /var/www/erpcore2
sudo chmod +x /var/www/erpcore2/ERPCore2

# Update service file
sudo tee /etc/systemd/system/erpcore2.service > /dev/null <<EOF
[Unit]
Description=ERP Core 2 Application
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/var/www/erpcore2
ExecStart=/var/www/erpcore2/ERPCore2
Restart=always
RestartSec=10
SyslogIdentifier=erpcore2
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
EOF

# Reload and start service
sudo systemctl daemon-reload
sudo systemctl enable erpcore2
sudo systemctl start erpcore2

echo "Deployment completed successfully!"
echo "Check service status: sudo systemctl status erpcore2"
