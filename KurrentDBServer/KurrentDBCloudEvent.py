#!/usr/bin/env python3
"""
CloudEvent HTTP Server for KurrentDB/EventStoreDB
Receives CloudEvent messages via HTTP and stores them in KurrentDB
"""

import os
import sys
import json
import uuid
import statistics
from datetime import datetime
from typing import Dict, Any, List
from collections import deque
from contextlib import asynccontextmanager

from fastapi import FastAPI, HTTPException, Request
from fastapi.responses import JSONResponse
import uvicorn
import requests
from pydantic import BaseModel, Field, ConfigDict

# --------------------------------------------------------------------------
# Configuration from environment variables
# --------------------------------------------------------------------------
KURRENTDB_USERNAME = os.getenv("KURRENTDB_USERNAME", "")
KURRENTDB_PASSWORD = os.getenv("KURRENTDB_PASSWORD", "")
KURRENTDB_HOST = os.getenv("KURRENTDB_HOST", "")
KURRENTDB_PORT = os.getenv("KURRENTDB_PORT", "")
KURRENTDB_STREAM_NAME = os.getenv("KURRENTDB_STREAM_NAME", "EnergyEventStream")
KURRENTDB_EVENT_TYPE = os.getenv("KURRENTDB_EVENT_TYPE", "CloudEvent")

# --------------------------------------------------------------------------
# Statistics Tracker
# --------------------------------------------------------------------------
class StatisticsTracker:
    """Tracks message processing statistics"""

    def __init__(self):
        self.message_count = 0
        self.timestamps: deque = deque(maxlen=1000)
        self.intervals: List[float] = []

    def record_message(self):
        """Record a new message reception"""
        now = datetime.now()
        self.message_count += 1

        if self.timestamps:
            interval = (now - self.timestamps[-1]).total_seconds()
            self.intervals.append(interval)

            # Keep only last 1000 intervals for statistics
            if len(self.intervals) > 1000:
                self.intervals.pop(0)

        self.timestamps.append(now)

        # Progress indicator: print '.' every 100 messages
        if self.message_count % 100 == 0:
            print('.', end='', flush=True)

        # Statistics output every 1000 messages
        if self.message_count % 1000 == 0:
            self.print_statistics()

    def print_statistics(self):
        """Print statistics for last 1000 messages"""
        if len(self.intervals) < 2:
            return

        # Calculate frequency (messages per second)
        frequencies = [1.0 / interval if interval > 0 else 0 for interval in self.intervals]

        min_freq = min(frequencies)
        max_freq = max(frequencies)
        avg_freq = statistics.mean(frequencies)
        stddev_freq = statistics.stdev(frequencies) if len(frequencies) > 1 else 0

        print()  # New line after dots
        print(f"\n{'=' * 70}")
        print(f"Statistics after {self.message_count} messages:")
        print(f"{'=' * 70}")
        print(f"Frequency (msg/sec):")
        print(f"  Min:    {min_freq:>10.2f}")
        print(f"  Max:    {max_freq:>10.2f}")
        print(f"  Avg:    {avg_freq:>10.2f}")
        print(f"  StdDev: {stddev_freq:>10.2f}")
        print(f"{'=' * 70}\n")


# --------------------------------------------------------------------------
# Global instances
# --------------------------------------------------------------------------
stats_tracker = StatisticsTracker()


# --------------------------------------------------------------------------
# Pydantic Models
# --------------------------------------------------------------------------
class CloudEventModel(BaseModel):
    """CloudEvent v1.0 specification model"""
    model_config = ConfigDict(
        json_schema_extra={
            "example": {
                "specversion": "1.0",
                "type": "com.example.energy.measurement",
                "source": "/sensors/meter-123",
                "id": "A234-1234-1234",
                "time": "2025-01-14T10:30:00Z",
                "datacontenttype": "application/json",
                "data": {
                    "power": 1250.5,
                    "voltage": 230.2,
                    "current": 5.43
                }
            }
        }
    )

    specversion: str = Field(default="1.0", description="CloudEvents version")
    type: str = Field(..., description="Event type")
    source: str = Field(..., description="Event source")
    id: str = Field(default_factory=lambda: str(uuid.uuid4()), description="Event ID")
    time: str = Field(default_factory=lambda: datetime.utcnow().isoformat() + "Z", description="Event timestamp")
    datacontenttype: str = Field(default="application/json", description="Content type")
    data: Dict[str, Any] = Field(..., description="Event data")


# --------------------------------------------------------------------------
# JSON Serialization Helper
# --------------------------------------------------------------------------
def json_default(obj):
    """JSON serializer for objects not serializable by default"""
    if isinstance(obj, datetime):
        return obj.isoformat()
    raise TypeError(f"Type {type(obj)} not serializable")


# --------------------------------------------------------------------------
# KurrentDB HTTP Client
# --------------------------------------------------------------------------
def append_to_kurrentdb(events: List[Dict[str, Any]]) -> requests.Response:
    """
    Append events to KurrentDB stream via HTTP

    Args:
        events: List of event dictionaries with eventId, eventType, data, metadata

    Returns:
        Response from KurrentDB
    """
    url = f"http://{KURRENTDB_HOST}:{KURRENTDB_PORT}/streams/{KURRENTDB_STREAM_NAME}"

    response = requests.post(
        url,
        auth=(KURRENTDB_USERNAME, KURRENTDB_PASSWORD),
        headers={"Content-Type": "application/vnd.eventstore.events+json"},
        data=json.dumps(events, ensure_ascii=False, default=json_default),
        timeout=10,
    )

    response.raise_for_status()
    return response


def test_kurrentdb_connection() -> bool:
    """Test connection to KurrentDB"""
    try:
        url = f"http://{KURRENTDB_HOST}:{KURRENTDB_PORT}/info"
        response = requests.get(
            url,
            auth=(KURRENTDB_USERNAME, KURRENTDB_PASSWORD),
            timeout=5
        )
        response.raise_for_status()
        print(f"✓ Connected to KurrentDB at {KURRENTDB_HOST}:{KURRENTDB_PORT}")
        return True
    except Exception as e:
        print(f"✗ Failed to connect to KurrentDB: {e}", file=sys.stderr)
        return False


# --------------------------------------------------------------------------
# Lifespan Handler
# --------------------------------------------------------------------------
@asynccontextmanager
async def lifespan(app: FastAPI):
    """Lifespan event handler"""
    # Startup
    print("\n" + "=" * 70)
    print("CloudEvent Server started")
    print("=" * 70)
    print(f"KurrentDB Host:   {KURRENTDB_HOST}:{KURRENTDB_PORT}")
    print(f"Stream Name:      {KURRENTDB_STREAM_NAME}")
    print(f"Event Type:       {KURRENTDB_EVENT_TYPE}")
    print("=" * 70)
    test_kurrentdb_connection()
    print("=" * 70 + "\n")

    yield

    # Shutdown
    print("\nCloudEvent Server shutting down...")


# --------------------------------------------------------------------------
# FastAPI Application
# --------------------------------------------------------------------------
app = FastAPI(
    title="CloudEvent Server",
    description="HTTP server for receiving CloudEvents and storing in KurrentDB",
    version="1.0.0",
    lifespan=lifespan
)


@app.get("/")
async def root():
    """Root endpoint with server information"""
    return {
        "service": "CloudEvent Server",
        "version": "1.0.0",
        "status": "running",
        "messages_processed": stats_tracker.message_count,
        "endpoints": {
            "health": "/health",
            "metrics": "/metrics",
            "event": "/event (POST)"
        }
    }


@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {
        "status": "healthy",
        "timestamp": datetime.utcnow().isoformat() + "Z"
    }


@app.get("/metrics")
async def metrics():
    """Metrics endpoint"""
    return {
        "messages_processed": stats_tracker.message_count,
        "timestamp": datetime.utcnow().isoformat() + "Z"
    }


@app.post("/event")
async def receive_event(cloud_event: CloudEventModel):
    """
    Receive a CloudEvent and store it in KurrentDB

    Args:
        cloud_event: CloudEvent message following CloudEvents v1.0 spec

    Returns:
        Success response with event ID
    """
    try:
        # Create event for KurrentDB
        event = {
            "eventId": cloud_event.id,
            "eventType": KURRENTDB_EVENT_TYPE,
            "data": cloud_event.model_dump(),
            "metadata": {
                "content-type": cloud_event.datacontenttype,
                "cloud-event-id": cloud_event.id,
                "cloud-event-type": cloud_event.type,
                "cloud-event-source": cloud_event.source,
                "cloud-event-time": cloud_event.time
            }
        }

        # Append to stream (KurrentDB expects an array)
        append_to_kurrentdb([event])

        # Record statistics
        stats_tracker.record_message()

        return JSONResponse(
            status_code=201,
            content={
                "status": "success",
                "message": "Event stored successfully",
                "event_id": cloud_event.id,
                "stream": KURRENTDB_STREAM_NAME
            }
        )

    except requests.exceptions.RequestException as e:
        print(f"\n✗ HTTP Error storing event: {e}", file=sys.stderr)
        raise HTTPException(
            status_code=500,
            detail=f"Failed to store event in KurrentDB: {str(e)}"
        )
    except Exception as e:
        print(f"\n✗ Error storing event: {e}", file=sys.stderr)
        raise HTTPException(
            status_code=500,
            detail=f"Failed to store event: {str(e)}"
        )


@app.post("/event/batch")
async def receive_event_batch(events: List[CloudEventModel]):
    """
    Receive multiple CloudEvents and store them in KurrentDB

    Args:
        events: List of CloudEvent messages

    Returns:
        Success response with number of events stored
    """
    try:
        # Create events for KurrentDB
        kurrent_events = []
        for cloud_event in events:
            event = {
                "eventId": cloud_event.id,
                "eventType": KURRENTDB_EVENT_TYPE,
                "data": cloud_event.model_dump(),
                "metadata": {
                    "content-type": cloud_event.datacontenttype,
                    "cloud-event-id": cloud_event.id,
                    "cloud-event-type": cloud_event.type,
                    "cloud-event-source": cloud_event.source,
                    "cloud-event-time": cloud_event.time
                }
            }
            kurrent_events.append(event)

        # Append batch to stream
        append_to_kurrentdb(kurrent_events)

        # Record statistics for each event
        for _ in events:
            stats_tracker.record_message()

        return JSONResponse(
            status_code=201,
            content={
                "status": "success",
                "message": f"{len(events)} events stored successfully",
                "count": len(events),
                "stream": KURRENTDB_STREAM_NAME
            }
        )

    except requests.exceptions.RequestException as e:
        print(f"\n✗ HTTP Error storing events: {e}", file=sys.stderr)
        raise HTTPException(
            status_code=500,
            detail=f"Failed to store events in KurrentDB: {str(e)}"
        )
    except Exception as e:
        print(f"\n✗ Error storing events: {e}", file=sys.stderr)
        raise HTTPException(
            status_code=500,
            detail=f"Failed to store events: {str(e)}"
        )


# --------------------------------------------------------------------------
# Main entry point
# --------------------------------------------------------------------------
if __name__ == "__main__":
    import sys
    import os

    # Get the module name from the file name (without .py extension)
    module_name = os.path.splitext(os.path.basename(__file__))[0]

    uvicorn.run(
        f"{module_name}:app",
        host="0.0.0.0",
        port=8000,
        log_level="info",
        access_log=True,
        reload=False
    )
