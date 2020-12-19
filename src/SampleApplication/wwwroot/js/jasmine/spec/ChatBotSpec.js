describe("ChatBotService", function () {

    const testUserName = "Test Osteron";
    const chatBotName = "ChatBot 1.0";

    it("Should return a friendly message with message type 0", async function () {
        let response = await apiCall("/api/ChatBot/SayHello", {
            Type: 0, // Normal
            Name: testUserName,
        });

        expect(response.helloMessage).toEqual(`Hello ${testUserName}.`);
        expect(response.from).toEqual(chatBotName);
    });

    it("Should return a cacual message with type 1", async function () {
        let response = await apiCall("/api/ChatBot/SayHello", {
            Type: 1, // Casual
            Name: testUserName,
        });

        expect(response.helloMessage).toEqual(`Hey ${testUserName}, what up?`);
        expect(response.from).toEqual(chatBotName);
    });

    it("Should return a rude message with type 2", async function () {
        let response = await apiCall("/api/ChatBot/SayHello", {
            Type: 2, // Rude
            Name: testUserName,
        });

        expect(response.helloMessage).toEqual(`Talk to the hand ${testUserName}!`);
        expect(response.from).toEqual(chatBotName);
    });

    it("Should return an exception if no name is given", async function () {
        try {
            await apiCall("/api/ChatBot/SayHello", {
                Type: 0, // Normal
                Name: null,
            });
            fail("Service call expected to fail");
        } catch (e) {
            const exceptionMessage = "System.ArgumentException: Request must contain a non-empty Name";
            expect(e.responseText.substring(0, exceptionMessage.length)).toEqual(exceptionMessage);
        };
    });

});
