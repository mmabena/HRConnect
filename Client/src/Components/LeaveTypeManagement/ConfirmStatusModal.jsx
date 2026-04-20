import "./confirm-modal.css";

const ConfirmStatusModal = ({ isOpen, onClose, onConfirm, isActive }) => {
  if (!isOpen) return null;

  return (
    <div className="confirm-overlay" onClick={onClose}>
      <div className="confirm-card" onClick={(e) => e.stopPropagation()}>

        <div className="logo-container">
          <span className="logo-bold">singular</span>
          <span className="logo-light">express</span>
        </div>

        <h3 className="confirm-title">
          {isActive ? "Deactivate Leave Type" : "Activate Leave Type"}
        </h3>

        <p className="confirm-text">
          Are you sure you want to {isActive ? "deactivate" : "activate"} this leave type?
          <br />
          This will affect employee entitlements.
        </p>

        <div className="confirm-actions">
          <button className="confirm-cancel" onClick={onClose}>
            Cancel
          </button>

          <button className="confirm-confirm" onClick={onConfirm}>
            Confirm
          </button>
        </div>

      </div>
    </div>
  );
};

export default ConfirmStatusModal;