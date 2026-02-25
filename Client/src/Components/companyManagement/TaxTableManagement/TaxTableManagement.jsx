import React, { useState, useEffect } from "react";
import api from "../../../api/api";
import "../../../styles/global.css";
import TaxTableUpload from "./TaxTableUpload";

function TaxTableManagement() {
  const [activeTable, setActiveTable] = useState(null);
  const [previousTables, setPreviousTables] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showUploadPopup, setShowUploadPopup] = useState(false);

  const fetchTaxTables = async () => {
    try {
      const response = await api.get("/TaxTableUpload");

      const tables = Array.isArray(response.data)
        ? response.data
        : response.data?.data || [];

      const active = tables.find(t => t.effectiveTo === null);
      const previous = tables.filter(t => t.effectiveTo !== null);

      setActiveTable(active || null);
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

  if (loading) return <div>Loading...</div>;

  return (
    <div className="menu-background custom-scrollbar">
      
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
              <div>
                <strong>Status:</strong>{" "}
                <span className="status-badge active">Active</span>
              </div>
              <div>
                <strong>Effective From:</strong>{" "}
                {new Date(activeTable.effectiveFrom).toLocaleDateString()}
              </div>
            </div>

            <table className="custom-table">
              <thead>
                <tr>
                  <th>File Name</th>
                  <th>Tax Year</th>
                  <th>Effective From</th>
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
                      className="link-btn"
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
        <div className="card-header">
          <h3>Previous Tax Tables</h3>
        </div>

        <table className="custom-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Tax Year</th>
              <th>Effective From</th>
              <th>Effective To</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            {previousTables.length > 0 ? (
              previousTables.map((table) => (
                <tr key={table.id}>
                  <td>{table.fileName}</td>
                  <td>{table.taxYear}</td>
                  <td>{new Date(table.effectiveFrom).toLocaleDateString()}</td>
                  <td>
                    {table.effectiveTo
                      ? new Date(table.effectiveTo).toLocaleDateString()
                      : "-"}
                  </td>
                  <td>
                    <a
                      href={table.fileUrl}
                      target="_blank"
                      rel="noreferrer"
                      className="link-btn"
                    >
                      View
                    </a>
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan="5">No previous tax tables found</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* UPLOAD POPUP */}
      {showUploadPopup && (
        <div className="modal-overlay">
          <div className="modal-content">
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
        </div>
      )}
    </div>
  );
}

export default TaxTableManagement;