import React, { useState, useEffect } from "react";
import api from "../../../api/api.js";
import "../../../styles/global.css";
import TaxTableUpload from "./TaxTableUpload";
import NavBar from "../../NavBar.jsx";

function TaxTableManagement() {
  const [activeTable, setActiveTable] = useState(null);
  const [previousTables, setPreviousTables] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showUploadPopup, setShowUploadPopup] = useState(false);
  const [currentTime, setCurrentTime] = useState("");
  const [currentDate, setCurrentDate] = useState("");

 const fetchTaxTables = async () => {
  try {
    const response = await api.get("/taxtableupload");
    const tables = Array.isArray(response.data)
      ? response.data
      : response.data?.data || [];

    if (tables.length === 0) {
      setActiveTable(null);
      setPreviousTables([]);
      return;
    }

    // Sort by effectiveFrom DESC (newest first)
    const sorted = [...tables].sort(
      (a, b) => new Date(b.effectiveFrom) - new Date(a.effectiveFrom)
    );

    const active = sorted[0]; // newest is active
    const previous = sorted.slice(1);

    setActiveTable(active);
    setPreviousTables(previous);

  } catch (err) {
    console.error("API ERROR:", err.response?.data || err.message);
    setActiveTable(null);
    setPreviousTables([]);
  } finally {
    setLoading(false);
  }
};

  useEffect(() => {
    fetchTaxTables();
  }, []);

  useEffect(() => {
    const now = new Date();
    setCurrentTime(now.toLocaleTimeString("en-US", { hour: "numeric", minute: "2-digit", hour12: true }));
    setCurrentDate(now.toLocaleDateString("en-US", { year: "numeric", month: "short", day: "numeric" }));
  }, []);

  if (loading) return <div>Loading...</div>;

  return (
    <div className="menu-background custom-scrollbar">
      <div className="wrapper-container">
        <div className="company-heading-container">
          Tax Table Management
          <div className="icon">
            <img src="/images/notifications.png" alt="Notification Icon" className="heading-icon" />
            <div className="util-box m-box">{currentDate}</div>
            <div className="util-box s-box">{currentTime}</div>
          </div>
        </div>
      </div>
      <NavBar />

      {/* ACTIVE TAX TABLE */}
     <div className="card-container">
        <div className="card-header">
          <h3>Active Tax Table</h3>
          <button className="upload-btn" onClick={() => setShowUploadPopup(true)}>
            Upload Tax Table
          </button>
        </div>

        {activeTable ? (
            <>
              <div className="status-row">
                <div className="status-left">
                  <span className="label">Status :</span>
                  <span className="status-badge active">Active</span>
                </div>

                <div className="status-right">
                  <span className="label">Effective Date:</span>
                  <span className="date-value">
                    {new Date(activeTable.effectiveFrom).toLocaleDateString("en-US", {
                      year: "numeric",
                      month: "long",
                      day: "numeric",
                    })}
                  </span>
                </div>
              </div>

             <table className="custom-table">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Year</th>
                    <th>Action</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>{activeTable.fileName}</td>
                    <td>{activeTable.taxYear}</td>
                    <td>
                      <a
                        href={activeTable.fileUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="view-link"
                      >
                        View
                      </a>
                    </td>
                  </tr>
                </tbody>
              </table>
            </>
          ) : (
            <p>No active tax table found.</p>
          )}
        </div>

      {/* PREVIOUS TAX TABLES */}
      <div className="card-container">
        <div className="card-header"><h3>Previous Tax Tables</h3></div>

        <table className="custom-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Year</th>
            </tr>
          </thead>
          <tbody>
            {previousTables.length > 0 ? (
              previousTables.map((table) => (
                <tr key={table.id}>
                  <td>{table.fileName}</td>
                  <td>{table.taxYear}</td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan="2">No previous tax tables found</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* UPLOAD POPUP */}
      {showUploadPopup && (
        <div className="modal-overlay">
          <TaxTableUpload
            existingYears={[
              ...(activeTable ? [activeTable.taxYear] : []),
              ...previousTables.map(t => t.taxYear),
            ]}
            onClose={() => setShowUploadPopup(false)}
            onUploadSuccess={() => {
              fetchTaxTables();
              setShowUploadPopup(false);
            }}
          />
        </div>
      )}
    </div>
  );
}

export default TaxTableManagement;