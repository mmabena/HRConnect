import { useState, useEffect } from "react";
import "./annual-editor.css";
import {
  updateLeaveType,
  previewEntitlementImpact,
} from "../../api/leaveTypeApi";
import { useNavigate } from "react-router-dom";

const AnnualLeaveEditor = ({
  leaveType,
  onSuccess,
  onClose,
  isEditing,
  setIsEditing,
}) => {
  const [activeTab, setActiveTab] = useState("groupA");
  const [editedRules, setEditedRules] = useState({});
  const [customRules, setCustomRules] = useState([]);
  const [message, setMessage] = useState("");
  const [messageType, setMessageType] = useState("");
  const [errorFields, setErrorFields] = useState({});
  const [isCheckingImpact, setIsCheckingImpact] = useState(false);
  const [affectedEmployees, setAffectedEmployees] = useState([]);
  const [lastEditedRuleKey, setLastEditedRuleKey] = useState(null);
  const navigate = useNavigate();

  const buildGroupedRules = () => {
    return {
      groupA: leaveType.rules.filter((r) => r.groupKey === "GROUP_A"),
      senior: leaveType.rules.filter((r) => r.groupKey === "SENIOR"),
      executive: leaveType.rules.filter((r) => r.groupKey === "EXECUTIVE"),
    };
  };

  const grouped = buildGroupedRules();

  const baseRules =
    activeTab === "groupA"
      ? grouped.groupA
      : activeTab === "senior"
        ? grouped.senior
        : grouped.executive;

  const currentRules = [
    ...baseRules,
    ...customRules.filter((r) => {
      if (activeTab === "groupA") return r.groupKey === "GROUP_A";
      if (activeTab === "senior") return r.groupKey === "SENIOR";
      return r.groupKey === "EXECUTIVE";
    }),
  ];
  const getRuleKey = (rule) => {
    return (
      rule.id ??
      `${rule.groupKey}-${rule.minYearsService}-${rule.maxYearsService}`
    );
  };
  const handleFieldChange = (ruleKey, field, value) => {
    setLastEditedRuleKey(ruleKey);
    setEditedRules((prev) => ({
      ...prev,
      [ruleKey]: {
        ...prev[ruleKey],
        [field]: value,
      },
    }));
    setErrorFields((prev) => {
      const copy = { ...prev };
      delete copy[ruleKey];
      return copy;
    });
  };
  const handleAddRange = () => {
    const groupKey =
      activeTab === "groupA"
        ? "GROUP_A"
        : activeTab === "senior"
          ? "SENIOR"
          : "EXECUTIVE";

    setCustomRules((prev) => [
      ...prev,
      {
        id: "new-" + Date.now(),
        groupKey: groupKey,
        minYearsService: 0,
        maxYearsService: null,
        daysAllocated: 0,
      },
    ]);
  };
  const hasChanges =
    Object.keys(editedRules).length > 0 || customRules.length > 0;

  const showMessage = (text, type = "error") => {
    setMessage(text);
    setMessageType(type);
  };

  const handleRemoveRange = (id) => {
    setCustomRules((prev) => prev.filter((r) => r.id !== id));
  };
  const mapBackendError = (message) => {
    if (!message) return "Something went wrong";

    const msg = message.toLowerCase();

    if (msg.includes("daysallocated must be greater")) {
      return "Leave days must be greater than 0";
    }

    if (msg.includes("cannot decrease")) {
      return "Leave days cannot decrease as years of service increases";
    }

    if (msg.includes("maxyearsservice") && msg.includes("less")) {
      return "Max years cannot be less than minimum years";
    }

    if (msg.includes("gap detected")) {
      return "Employment year ranges must have no gaps between them";
    }

    if (msg.includes("overlapping")) {
      return "Employment year ranges cannot overlap";
    }

    if (msg.includes("duplicate")) {
      return "Duplicate year service ranges are not allowed";
    }

    return "Unable to save changes. Please check your inputs.";
  };
  const validateFrontendRules = () => {
    const payload = buildPayload();

    for (const rule of payload.rules) {
      if (rule.daysAllocated <= 0) {
        return "Leave days must be greater than 0";
      }

      if (
        rule.maxYearsService !== null &&
        rule.maxYearsService < rule.minYearsService
      ) {
        return "Max years cannot be less than minimum years";
      }
    }

    return null;
  };
  const buildPayload = () => {
    const allRules = [...leaveType.rules, ...customRules];

    const finalRules = allRules.map((r) => {
      const ruleKey = getRuleKey(r);

      const edited = editedRules[ruleKey];

      const min =
        edited?.minYearsService !== undefined
          ? Number(edited.minYearsService)
          : r.minYearsService;

      const max =
        edited?.maxYearsService !== undefined
          ? edited.maxYearsService === ""
            ? null
            : Number(edited.maxYearsService)
          : r.maxYearsService;

      const days =
        edited?.daysAllocated !== undefined
          ? Number(edited.daysAllocated)
          : r.daysAllocated;

      return {
        groupKey: r.groupKey,
        minYearsService: Number(min.toFixed(2)),
        maxYearsService: max !== null ? Number(max.toFixed(2)) : null,
        daysAllocated: Number(days.toFixed(2)),
      };
    });
    return {
      name: leaveType.name,
      description: leaveType.description,
      femaleOnly: leaveType.femaleOnly,
      isActive: leaveType.isActive,
      rules: finalRules,
    };
  };
  useEffect(() => {
    const checkImpact = async () => {
      if (!hasChanges) {
        setAffectedEmployees([]);
        return;
      }

      try {
        const payload = buildPayload();

        const previewData = await previewEntitlementImpact(payload);

        setAffectedEmployees(previewData || []);
      } catch (err) {
        console.error(err);
      }
    };

    checkImpact();
  }, [editedRules, customRules]);
  const allRules = [...leaveType.rules, ...customRules];

  const changedRules = allRules.filter((r) => {
    const ruleKey = getRuleKey(r);
    const edited = editedRules[ruleKey];

    if (!edited) return false;

    return (
      (edited.minYearsService !== undefined &&
        Number(edited.minYearsService) !== r.minYearsService) ||
      (edited.maxYearsService !== undefined &&
        Number(edited.maxYearsService) !== r.maxYearsService) ||
      (edited.daysAllocated !== undefined &&
        Number(edited.daysAllocated) !== r.daysAllocated)
    );
  });

  const totalChangedRules = changedRules.length;

  const changedGroups = [...new Set(changedRules.map((r) => r.groupKey))];

  const formattedGroups = changedGroups.map((group) => {
    if (group === "GROUP_A") return "Unskilled-Middle";
    if (group === "SENIOR") return "Senior";
    if (group === "EXECUTIVE") return "Executive";

    return group;
  });

  const shouldNavigateToAffectedEmployees = affectedEmployees.length > 0;
  return (
    <div className="annual-wrapper">
      {message && (
        <div className={`message-text ${messageType}`}>{message}</div>
      )}
      <div className="tabs">
        <button
          className={activeTab === "groupA" ? "active" : ""}
          onClick={() => setActiveTab("groupA")}
        >
          Unskilled-Middle
        </button>

        <button
          className={activeTab === "senior" ? "active" : ""}
          onClick={() => setActiveTab("senior")}
        >
          Senior
        </button>

        <button
          className={activeTab === "executive" ? "active" : ""}
          onClick={() => setActiveTab("executive")}
        >
          Executive
        </button>
      </div>

      <div className="rule-box">
        <div className="rule-header">
          <span>Min Years</span>
          <span>Max Years</span>
          <span>Leave Days</span>
        </div>

        <div className="rule-body">
          {currentRules.map((r) => {
            const ruleKey = getRuleKey(r);
            const edited = editedRules[ruleKey] || {};

            return (
              <div key={ruleKey} className="rule-row">
                <input
                  disabled={!isEditing}
                  value={
                    edited.minYearsService !== undefined
                      ? edited.minYearsService
                      : r.minYearsService
                  }
                  onChange={(e) =>
                    handleFieldChange(
                      ruleKey,
                      "minYearsService",
                      e.target.value,
                    )
                  }
                />

                <input
                  disabled={!isEditing}
                  value={
                    edited.maxYearsService !== undefined
                      ? edited.maxYearsService
                      : (r.maxYearsService ?? "")
                  }
                  onChange={(e) =>
                    handleFieldChange(
                      ruleKey,
                      "maxYearsService",
                      e.target.value,
                    )
                  }
                />

                <div className="days-edit">
                  <input
                    disabled={!isEditing}
                    value={
                      edited.daysAllocated !== undefined
                        ? edited.daysAllocated
                        : r.daysAllocated
                    }
                    onChange={(e) =>
                      handleFieldChange(
                        ruleKey,
                        "daysAllocated",
                        e.target.value,
                      )
                    }
                  />

                  {edited.daysAllocated &&
                    Number(edited.daysAllocated) !== r.daysAllocated && (
                      <span className="diff">
                        {r.daysAllocated} → {edited.daysAllocated}
                      </span>
                    )}

                  {isEditing && r.id?.startsWith("new-") && (
                    <button
                      className="remove-range"
                      onClick={() => handleRemoveRange(r.id)}
                    >
                      ✕
                    </button>
                  )}
                </div>
              </div>
            );
          })}
        </div>

        {isEditing && (
          <button className="add-range" onClick={handleAddRange}>
            + Add Range
          </button>
        )}
      </div>

      <div className="impact-box">
        {!hasChanges ? (
          "No changes yet"
        ) : affectedEmployees.length === 0 ? (
          <div className="impact-no-change">
            No employees will be affected by these changes.
          </div>
        ) : (
          <>
            <div className="impact-change-line">
              <span className="impact-change-text">You modified</span>

              <span className="impact-employee-count">{totalChangedRules}</span>

              <span className="impact-change-text">
                rule{totalChangedRules > 1 ? "s" : ""}
              </span>
            </div>

            <div className="impact-change-line">
              Affected groups: {formattedGroups.join(", ")}
            </div>

            <div className="impact-employee-line">
              This will affect{" "}
              <span className="impact-employee-count">
                {affectedEmployees.length}
              </span>{" "}
              employee
              {affectedEmployees.length > 1 ? "s" : ""}
            </div>
          </>
        )}
      </div>
      <div className="actions">
        {!isEditing ? (
          <>
            <button className="cancel" onClick={onClose}>
              Back
            </button>

            <button className="next" onClick={() => setIsEditing(true)}>
              Edit
            </button>
          </>
        ) : (
          <>
            <button className="cancel" onClick={onClose}>
              Cancel
            </button>

            <button
              className="next"
              disabled={!hasChanges}
              onClick={async () => {
                setIsCheckingImpact(true);

                try {
                  const validationError = validateFrontendRules();

                  if (validationError) {
                    showMessage(validationError, "error");
                    return;
                  }
                  const payload = buildPayload();

                  const previewData = await previewEntitlementImpact(payload);

                  if (previewData.length === 0) {
                    await updateLeaveType(leaveType.id, payload);

                    showMessage(
                      "Leave entitlement updated successfully",
                      "success",
                    );

                    setTimeout(() => {
                      onSuccess();
                      onClose();
                    }, 800);

                    return;
                  }

                  navigate("/affected-employees", {
                    state: {
                      employees: previewData,
                      payload,
                      leaveTypeId: leaveType.id,
                    },
                  });
                } catch (err) {
                  console.error(err);

                  let message = err.response?.data;

                  if (typeof message === "object") {
                    message = message.title || JSON.stringify(message);
                  }

                  const friendlyMessage = mapBackendError(message);

                  showMessage(friendlyMessage, "error");
                } finally {
                  setIsCheckingImpact(false);
                }
              }}
            >
              {shouldNavigateToAffectedEmployees
                ? "Next: View Affected Employees"
                : "Save Changes"}
            </button>
          </>
        )}
      </div>
    </div>
  );
};

export default AnnualLeaveEditor;
