import { useState } from 'react'

function App() {
  const [count, setCount] = useState(0)

  return (
    <div className="min-h-screen bg-background flex items-center justify-center">
      <div className="text-center">
        <h1 className="text-4xl font-bold mb-4">Apivia</h1>
        <p className="text-xl text-muted-foreground mb-8">
          API First Design Platform
        </p>
        <button
          onClick={() => setCount((count) => count + 1)}
          className="px-6 py-3 bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-colors"
        >
          Count is {count}
        </button>
        <p className="mt-8 text-sm text-muted-foreground">
          Phase 0: Infrastructure Setup Complete ✅
        </p>
        <p className="mt-2 text-sm text-muted-foreground">
          Ready for Phase 1: Core Backend Services
        </p>
      </div>
    </div>
  )
}

export default App
