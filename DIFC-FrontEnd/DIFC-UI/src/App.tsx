import { MantineProvider } from "@mantine/core";
import Register from "./pages/Register";

function App() {
  return (
    <MantineProvider>
      <Register />
    </MantineProvider>
  );
}

export default App;