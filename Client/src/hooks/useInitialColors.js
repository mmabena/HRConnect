const COLORS = ["blue", "teal", "lime", "dark-blue"];

const useInitialColors = () => {
  const getInitialColorByIndex = (index) => {
    return COLORS[index % COLORS.length];
  };

  return { COLORS, getInitialColorByIndex };
};

export default useInitialColors;