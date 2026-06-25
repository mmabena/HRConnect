export const getDependentCounts = (deps = []) => {
  let principalCount = 1;
  let adultCount = 0;
  let childrenCount = 0;

  deps.forEach((d) => {
    const rel = (d.relationship || "").toLowerCase();

    if (rel === "spouse" || rel === "parent" || rel === "sibling") {
      adultCount++;
    } else if (rel === "child") {
      childrenCount++;
    } else {
      adultCount++;
    }
  });

  return {
    principal: principalCount,
    adult: adultCount,
    childrenCount,
  };
};