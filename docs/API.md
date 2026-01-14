# WinEcoSensor CloudEvents API

WinEcoSensor sends status and heartbeat messages using the [CloudEvents](https://cloudevents.io/) specification (v1.0).

## Endpoints

Configure the backend URL in the Settings dialog or `WinEcoSensor.config`:

```xml
<Backend>
  <Url>https://your-backend.example.com/api/events</Url>
</Backend>
```

## Event Types

### Status Event

Sent periodically (default: every 60 seconds) with complete system status.

**Type**: `eu.unlockeurope.energy.sensor.status`

**Example**:

```json
{
  "specversion": "1.0",
  "type": "eu.unlockeurope.energy.sensor.status",
  "source": "winecosensor://WORKSTATION-01/WinEcoSensor",
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "time": "2025-01-14T12:00:00.000Z",
  "datacontenttype": "application/json",
  "data": {
    "hardware": {
      "mainboardManufacturer": "ASUSTeK COMPUTER INC.",
      "mainboardModel": "PRIME B550M-A",
      "cpuName": "AMD Ryzen 5 5600X 6-Core Processor",
      "cpuCores": 6,
      "cpuBaseClockMHz": 3700,
      "totalMemoryMB": 32768,
      "gpus": [
        "NVIDIA GeForce RTX 3060"
      ],
      "monitors": [
        {
          "name": "DELL U2722D",
          "manufacturer": "Dell Inc.",
          "width": 2560,
          "height": 1440,
          "refreshRate": 60,
          "eprelNumber": "1234567",
          "powerOnWatts": 22.5,
          "powerStandbyWatts": 0.3
        }
      ],
      "disks": [
        {
          "model": "Samsung SSD 980 PRO 1TB",
          "sizeGB": 953,
          "mediaType": "SSD",
          "powerActiveWatts": 8.0,
          "powerIdleWatts": 0.05
        }
      ]
    },
    "userActivity": {
      "isLoggedIn": true,
      "userName": "john.doe",
      "domainName": "CORP",
      "sessionState": "Active",
      "firstActivityUtc": "2025-01-14T08:30:00.000Z",
      "activityDuration": "03:30:00",
      "idleTimeSeconds": 45,
      "isScreenSaverActive": false,
      "isWorkstationLocked": false
    },
    "displayState": {
      "monitorCount": 1,
      "currentState": "On",
      "stateChangedUtc": "2025-01-14T08:30:00.000Z",
      "todayOnTime": "03:25:00",
      "todayOffTime": "00:00:00",
      "todayIdleTime": "00:05:00"
    },
    "remoteAccess": {
      "isRdpConnected": false,
      "rdpClientAddress": null,
      "isTeamViewerRunning": false,
      "isAnyDeskRunning": false,
      "isVncRunning": false,
      "otherRemoteTools": []
    },
    "energyState": {
      "timestampUtc": "2025-01-14T12:00:00.000Z",
      "cpuPowerWatts": 35.5,
      "displayPowerWatts": 22.5,
      "gpuPowerWatts": 40.0,
      "diskPowerWatts": 2.5,
      "memoryPowerWatts": 12.0,
      "basePowerWatts": 30.0,
      "totalPowerWatts": 168.2,
      "psuEfficiency": 0.85,
      "dailyEnergyWh": 589.5,
      "sessionEnergyWh": 589.5,
      "sessionDuration": "03:30:00",
      "efficiencyRating": "C"
    }
  }
}
```

### Heartbeat Event

Sent periodically (default: every 300 seconds) as a keep-alive signal.

**Type**: `eu.unlockeurope.energy.sensor.heartbeat`

**Example**:

```json
{
  "specversion": "1.0",
  "type": "eu.unlockeurope.energy.sensor.heartbeat",
  "source": "winecosensor://WORKSTATION-01/WinEcoSensor",
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "time": "2025-01-14T12:05:00.000Z",
  "datacontenttype": "application/json",
  "data": {
    "status": "running",
    "uptime": "03:35:00",
    "version": "1.0.0"
  }
}
```

## Data Schema Details

### Hardware Information

| Field | Type | Description |
|-------|------|-------------|
| `mainboardManufacturer` | string | Mainboard manufacturer name |
| `mainboardModel` | string | Mainboard model name |
| `cpuName` | string | Full CPU name |
| `cpuCores` | int | Number of CPU cores |
| `cpuBaseClockMHz` | int | Base clock speed in MHz |
| `totalMemoryMB` | long | Total RAM in megabytes |
| `gpus` | string[] | List of GPU names |
| `monitors` | MonitorInfo[] | Connected monitors |
| `disks` | DiskInfo[] | Storage devices |

### Monitor Information

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Monitor name/model |
| `manufacturer` | string | Manufacturer name |
| `width` | int | Horizontal resolution |
| `height` | int | Vertical resolution |
| `refreshRate` | int | Refresh rate in Hz |
| `eprelNumber` | string | EU Energy Label number |
| `powerOnWatts` | double | Power when on (from EPREL or estimated) |
| `powerStandbyWatts` | double | Standby power |

### User Activity

| Field | Type | Description |
|-------|------|-------------|
| `isLoggedIn` | bool | User logged in |
| `userName` | string | Username |
| `domainName` | string | Domain name |
| `sessionState` | string | "Active", "Locked", "Disconnected" |
| `firstActivityUtc` | datetime | First activity today (UTC) |
| `activityDuration` | timespan | Duration since first activity |
| `idleTimeSeconds` | int | Seconds since last input |
| `isScreenSaverActive` | bool | Screen saver running |
| `isWorkstationLocked` | bool | Workstation locked |

### Display State

| Field | Type | Description |
|-------|------|-------------|
| `monitorCount` | int | Number of monitors |
| `currentState` | string | "On", "Off", "Idle" |
| `stateChangedUtc` | datetime | Last state change |
| `todayOnTime` | timespan | Total on time today |
| `todayOffTime` | timespan | Total off time today |
| `todayIdleTime` | timespan | Total idle time today |

### Energy State

| Field | Type | Description |
|-------|------|-------------|
| `cpuPowerWatts` | double | CPU power consumption |
| `displayPowerWatts` | double | Display power consumption |
| `gpuPowerWatts` | double | GPU power consumption |
| `diskPowerWatts` | double | Storage power consumption |
| `memoryPowerWatts` | double | Memory power consumption |
| `basePowerWatts` | double | Base system power |
| `totalPowerWatts` | double | Total AC power (after PSU) |
| `psuEfficiency` | double | PSU efficiency factor |
| `dailyEnergyWh` | double | Energy today in Wh |
| `sessionEnergyWh` | double | Energy this session in Wh |
| `efficiencyRating` | string | A-G efficiency rating |

## Backend Implementation

### Minimal Backend Example (Node.js/Express)

```javascript
const express = require('express');
const app = express();

app.use(express.json());

app.post('/api/events', (req, res) => {
  const event = req.body;
  
  console.log(`Received ${event.type} from ${event.source}`);
  
  if (event.type === 'eu.unlockeurope.energy.sensor.status') {
    const { energyState, userActivity } = event.data;
    console.log(`Power: ${energyState.totalPowerWatts}W, User: ${userActivity.userName}`);
  }
  
  res.status(202).json({ accepted: true });
});

app.listen(3000, () => console.log('Backend listening on port 3000'));
```

### Database Schema Example (PostgreSQL)

```sql
CREATE TABLE sensor_events (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  event_id VARCHAR(100) NOT NULL,
  event_type VARCHAR(100) NOT NULL,
  source VARCHAR(255) NOT NULL,
  event_time TIMESTAMPTZ NOT NULL,
  hostname VARCHAR(100),
  username VARCHAR(100),
  total_power_watts DECIMAL(10,2),
  daily_energy_wh DECIMAL(10,2),
  efficiency_rating CHAR(1),
  raw_data JSONB,
  received_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_sensor_events_source ON sensor_events(source);
CREATE INDEX idx_sensor_events_time ON sensor_events(event_time);
```

## HTTP Headers

WinEcoSensor sends events with these headers:

```
Content-Type: application/cloudevents+json
User-Agent: WinEcoSensor/1.0.0
```

## Response Handling

| Status Code | Meaning | Action |
|-------------|---------|--------|
| 2xx | Success | Event accepted |
| 4xx | Client error | Log error, don't retry |
| 5xx | Server error | Retry with backoff |

## Testing

You can test the backend connection from the Settings dialog:
1. Open Settings → Backend Server
2. Enter your backend URL
3. Click "Test Connection"

This sends an HTTP HEAD request (or GET if HEAD fails) to verify connectivity.
